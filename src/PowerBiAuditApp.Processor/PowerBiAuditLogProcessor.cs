using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using PowerBiAuditApp.Processor.Extensions;
using PowerBiAuditApp.Processor.Models;
using SendGrid.Helpers.Mail;

namespace PowerBiAuditApp.Processor
{
    public static class PowerBiAuditLogProcessor
    {
        [FunctionName(nameof(PowerBiAuditLogProcessor))]
        public static async Task Run(
            [QueueTrigger("audit-log-queue", Connection = "StorageAccountQueueEndpoint")] string name,
            [Blob("audit-pre-processed-log-storage/{queueTrigger}", FileAccess.Read, Connection = "StorageAccountBlobEndpoint")] CloudBlockBlob auditBlobClient,
            [Blob("audit-processed-log-storage", Connection = "StorageAccountBlobEndpoint")] CloudBlobContainer processedLogClient,
            [SendGrid] IAsyncCollector<SendGridMessage> messageCollector,
            ILogger log
        )
        {
            AuditLog model;
            try
            {
                model = await ReadJson(name, auditBlobClient, log);
            }
            catch (Exception exception)
            {
                await FailWithError(messageCollector, $"Reading auditLog failed for: {name}", auditBlobClient, exception, log);
                return;
            }

            try
            {
                foreach (var result in model.Response.Results)
                {
                    var headerLookup = GetHeaderLookup(result);

                    foreach (var dataSet in result.Result.Data.Dsr.DataOrRow)
                    {
                        foreach (var (key, data) in dataSet.PrimaryRows.SelectMany(x => x))
                        {
                            await WriteCsv(name, auditBlobClient, processedLogClient, messageCollector, log, data, model.Request.Queries, $"Primary{key}", result, dataSet, headerLookup);
                        }

                        foreach (var (key, data) in dataSet.SecondaryRows.SelectMany(x => x))
                        {
                            await WriteCsv(name, auditBlobClient, processedLogClient, messageCollector, log, data, model.Request.Queries, $"Secondary{key}", result, dataSet, headerLookup);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                await FailWithError(messageCollector, $"Outputting auditLog failed for: {name}", auditBlobClient, exception, log);
                return;
            }

            await auditBlobClient.DeleteAsync();
        }

        private static async Task FailWithError(IAsyncCollector<SendGridMessage> messageCollector, string message, CloudBlockBlob auditBlobClient, Exception exception, ILogger log)
        {
            var sendGridMessage = new SendGridMessage {
                From = new EmailAddress(Environment.GetEnvironmentVariable("ErrorFromEmailAddress", EnvironmentVariableTarget.Process)),
                Subject = message,
                PlainTextContent = $"{message} with error {exception.Message}\r\nThe file can be found at: {auditBlobClient.Uri}\r\n Please investigate."
            };
            sendGridMessage.AddTo(new EmailAddress(Environment.GetEnvironmentVariable("ErrorToEmailAddress", EnvironmentVariableTarget.Process)));


            await messageCollector.AddAsync(sendGridMessage);
            await messageCollector.FlushAsync();
            log.LogError(exception, message);
        }



        /// <summary>
        /// Deserialize the json file
        /// </summary>
        private static async Task<AuditLog> ReadJson(string name, CloudBlockBlob auditBlobClient, ILogger log)
        {
            log.LogInformation("Processing {file}", name);
            var stream = await auditBlobClient.OpenReadAsync();
            using var streamReader = new StreamReader(stream, Encoding.UTF8);
            using JsonReader reader = new JsonTextReader(streamReader);

            var settings = new JsonSerializerSettings {
                MissingMemberHandling = MissingMemberHandling.Error,
                ContractResolver = new MissingMemberContractResolver()
            };

            var serializer = JsonSerializer.Create(settings);

            return serializer.Deserialize<AuditLog>(reader);
        }

        /// <summary>
        /// Locate and return all possible header descriptor values indexed against their lookup key
        /// </summary>
        private static Dictionary<string, DescriptorSelect> GetHeaderLookup(ResultElement result)
        {
            var headerLookup = result.Result.Data.Descriptor.Select
                .Where(x => x is not null)
                .ToDictionary(x => x.Value);


            var highlightKeys = result.Result.Data.Descriptor.Select
                .Where(x => x?.Highlight?.Value is not null)
                .ToDictionary(x => x.Highlight.Value);


            var withGroupKeys = result.Result.Data.Descriptor.Select
                .Where(x => x?.GroupKeys is not null)
                .ToArray();

            var calcKeys = withGroupKeys
                .SelectMany(x => x.GroupKeys)
                .Where(x => x.Calc is not null)
                .ToDictionary(x => x.Calc, x => withGroupKeys.FirstOrDefault(d => d.Value != x.Calc && d.GroupKeys.Any(g => g.Calc == x.Calc)))
                .Where(x => x.Value != null);

            return headerLookup.Concat(highlightKeys)
                .Concat(calcKeys)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Process the returned json and write it to a csv file
        /// </summary>
        private static async Task WriteCsv(string name, CloudBlockBlob auditBlobClient, CloudBlobContainer processedLogClient,
            IAsyncCollector<SendGridMessage> messageCollector, ILogger log, PowerBiDataRow[] data, Query[] queries, string key, ResultElement result, PowerBiDataSet dataSet,
            Dictionary<string, DescriptorSelect> headerLookup)
        {
            try
            {
                // If no data was returned don't log it
                if (!data.Any())
                {
                    log.LogWarning("Did not find any data in {fileName}:{key}", name, key);
                    return;
                }

                var filename = $"{name} ({result.JobId}-{dataSet.Name}-{key}).csv";
                var blob = processedLogClient.GetBlockBlobReference(filename);

                await using var stream = await blob.OpenWriteAsync();
                await using var writer = new StreamWriter(stream);
                await using var csvWriter = new CsvWriter(writer,
                    new CsvConfiguration(CultureInfo.InvariantCulture) { LeaveOpen = false, HasHeaderRecord = false });

                var headers = GetHeaders(data);

                WriteHeaders(headers, headerLookup, queries, csvWriter);
                WriteRows(data, headers, dataSet, csvWriter);

                await csvWriter.FlushAsync();
                log.LogInformation("Data written for {name} ({result.JobId}-{dataSet.Name}-{key}).csv", name, result.JobId,
                    dataSet.Name, key);
            }
            catch (Exception exception)
            {
                await FailWithError(messageCollector, $"Writing CSV data failed for: {name} ({result.JobId}-{dataSet.Name}-{key})", auditBlobClient, exception, log);
            }
        }

        /// <summary>
        /// Gets header records
        /// </summary>
        private static ColumnHeader[] GetHeaders(PowerBiDataRow[] data)
        {
            var headers = data.Single(x => x.ColumnHeaders is not null).ColumnHeaders;


            var subDataRows =
                data.SingleOrDefault(d => d.SubDataRows is not null && d.SubDataRows.Any(s => s.ColumnHeaders is not null))?
                    .SubDataRows;

            var subHeaders = subDataRows?
                .Where(x => x.ColumnHeaders is not null)
                .SelectMany(x => x.ColumnHeaders)
                .Where(c => c is not null).ToArray();


            var subHeaderSet = new List<ColumnHeader>();
            if (subHeaders?.Any() == true)
            {
                var subColumnCount = data.Where(d => d.SubDataRows is not null)
                    .Max(x => Math.Max(x.SubDataRows.Length, x.SubDataRows.Max(s => s.Index + 1 ?? 0)));

                for (var i = 0; i < subColumnCount; i++)
                {
                    var columnIndex = 0;
                    foreach (var subHeader in subHeaders)
                    {
                        var newSubHeader = subHeader.Clone();
                        newSubHeader.SubDataRowIndex = i;
                        newSubHeader.SubDataColumnIndex = columnIndex++;
                        subHeaderSet.Add(newSubHeader);
                    }
                }

                headers = headers.Concat(subHeaderSet).ToArray();
            }

            return headers;
        }
        /// <summary>
        /// Write Headers to csv
        /// </summary>
        private static void WriteHeaders(ColumnHeader[] headers, Dictionary<string, DescriptorSelect> headerLookup, Query[] queries, CsvWriter csvWriter)
        {
            // Write CSV headers
            foreach (var header in headers)
            {
                var headerName = GetHeaderName(headerLookup, queries, header);
                if (header.SubDataRowIndex is not null && headers.Count(x => x.NameIndex == header.NameIndex) > 1)
                {
                    csvWriter.WriteField($"{headerName}[{header.SubDataRowIndex + 1}]");
                    continue;
                }

                csvWriter.WriteField(headerName);
            }

            csvWriter.NextRecord();
        }

        /// <summary>
        /// Lookup the header name
        /// </summary>
        private static string GetHeaderName(Dictionary<string, DescriptorSelect> headerLookup, Query[] queries, ColumnHeader header)
        {
            var headerDescriptor = headerLookup[header.NameIndex];
            var suffix = string.Empty;
            if (headerDescriptor.Highlight?.Value == header.NameIndex)
                suffix = "(Highlight)";

            switch (headerDescriptor.Kind)
            {
                case DescriptorKind.Select:
                    return string.Join("---", headerDescriptor.GroupKeys.Select(g => g.Source.Property)) + suffix;
                case DescriptorKind.Grouping:
                    {
                        var selectProperty = queries
                            .SelectMany(q => q.QueryQuery.Commands.Select(c =>
                                c.SemanticQueryDataShapeCommand.Query.Select.SingleOrDefault(s =>
                                    s.Name == headerDescriptor.Name)))
                            .SingleOrDefault(q => q is not null)?.Measure?.Property;

                        if (selectProperty is not null)
                            return selectProperty + suffix;

                        return Regex.Replace(headerDescriptor.Name, @"^[^()]*\([^()]*\.([^().]*)\)[^()]*", "$1") + suffix;
                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(headerLookup));
            }
        }


        /// <summary>
        /// Write rows to csv
        /// </summary>
        private static void WriteRows(PowerBiDataRow[] data, ColumnHeader[] headers, PowerBiDataSet dataSet, CsvWriter csvWriter)
        {
            object[] previousCsvRow = null;
            foreach (var row in data)
            {
                previousCsvRow = GetRow(row, headers, dataSet.ValueDictionary, previousCsvRow);
                foreach (var rowData in previousCsvRow)
                {
                    csvWriter.WriteField(rowData);
                }

                csvWriter.NextRecord();
            }
        }

        /// <summary>
        /// Read and process row json to row data
        /// </summary>
        private static object[] GetRow(PowerBiDataRow row, ColumnHeader[] headers, Dictionary<string, string[]> valueDictionary, object[] previousCsvRow)
        {
            var rowDataIndex = 0;
            var csvRow = new object[headers.Length];

            var copyBitmask = row.RepeatBitmask ?? 0;
            var nullBitmask = row.NullBitmask ?? 0;
            var subRowDataIndexLookup = new Dictionary<int, int>();

            var totalRows = CountSetBits(copyBitmask) +
                            CountSetBits(nullBitmask) +
                            row.RowValues.Length +
                            row.ValueLookup.Count;

            if (totalRows != headers.Count(x => x.SubDataRowIndex is null))
                throw new ArgumentException($"Number of rows doesn't match the headers (rows: {totalRows} headers:{headers.Count(x => x.SubDataRowIndex is null)}");

            for (var index = 0; index < headers.Length; index++)
            {
                if (IsBitSet(copyBitmask, index))
                {
                    // this is a duplicate
                    csvRow[index] = previousCsvRow[index];
                    continue;
                }

                if (IsBitSet(nullBitmask, index))
                {
                    // this is a null
                    csvRow[index] = null;
                    continue;
                }

                if (row.ValueLookup.TryGetValue(headers[index].NameIndex, out var cellData))
                {
                    csvRow[index] = ParseRowValue(headers, valueDictionary, index, cellData);
                    continue;
                }

                if (row.RowValues.Length < rowDataIndex + 1 && row.SubDataRows != null)
                {
                    var header = headers[index];
                    var rowIndex = header.SubDataRowIndex ?? throw new NullReferenceException();
                    var columnIndex = header.SubDataColumnIndex ?? throw new NullReferenceException();

                    var subData = row.SubDataRows.SingleOrDefault(x => x.Index == rowIndex);
                    if (subData is null)
                    {
                        if (row.SubDataRows.Length > rowIndex)
                        {
                            subData = row.SubDataRows[rowIndex];
                        }

                        if (subData is null || subData.Index is not null)
                        {
                            csvRow[index] = 0;
                            continue;
                        }
                    }


                    if (IsBitSet(subData.RepeatBitmask ?? 0, columnIndex))
                    {
                        // this is a duplicate

                        if (rowIndex <= 0 || !row.SubDataRows.Any(x => x.Index is null || x.Index < rowIndex)) // data came from the previous row
                        {
                            var previousCsvIndex = headers
                                .Select((x, i) => (value: x, index: i))
                                .Where(x => x.value.SubDataColumnIndex == columnIndex)
                                .Max(x => x.index);

                            csvRow[index] = previousCsvRow[previousCsvIndex];
                            continue;
                        }

                        var previousIndex = headers
                            .Select((x, i) => (value: x, index: i))
                            .Single(x => x.value.SubDataColumnIndex == columnIndex && x.value.SubDataRowIndex == rowIndex - 1)
                            .index;

                        csvRow[index] = csvRow[previousIndex];
                        continue;
                    }

                    if (IsBitSet(subData.NullBitmask ?? 0, columnIndex))
                    {
                        // this is a null
                        csvRow[index] = null;
                        continue;
                    }


                    if (subData.ValueLookup.TryGetValue(header.NameIndex, out var subCellData))
                    {
                        csvRow[index] = headers[index].ColumnType switch {
                            ColumnType.String => subCellData,
                            ColumnType.Int => int.Parse(subCellData),
                            ColumnType.Double => double.Parse(subCellData),
                            _ => throw new ArgumentOutOfRangeException(nameof(headers), $"Unknown type {headers[index].ColumnType}")
                        };
                        continue;
                    }

                    if (!subRowDataIndexLookup.TryGetValue(rowIndex, out var subRowDataIndex))
                        subRowDataIndex = 0;


                    if (subData.RowValues is null) // no rows returned for sub-data (aka data only has headings)
                    {
                        csvRow[index] = 0;
                        continue;
                    }

                    var subDataValue = subData.RowValues[subRowDataIndex++];
                    csvRow[index] = ParseRowValue(headers, valueDictionary, index, subDataValue);

                    subRowDataIndexLookup[rowIndex] = subRowDataIndex;
                    continue;
                }

                var data = row.RowValues[rowDataIndex++];
                csvRow[index] = ParseRowValue(headers, valueDictionary, index, data);
            }

            return csvRow;
        }

        private static object ParseRowValue(ColumnHeader[] headers, Dictionary<string, string[]> valueDictionary, int index, RowValue data)
        {
            switch (headers[index].ColumnType)
            {
                case ColumnType.String:
                    {
                        if (data.Integer is not null)
                        {
                            // need to lookup
                            var lookup = headers[index].DataIndex;
                            return valueDictionary[lookup][data.Integer.Value];
                        }

                        return data.String;
                    }
                case ColumnType.Int:
                    {
                        if (data.Integer is null)
                            throw new NullReferenceException();

                        return data.Integer;
                    }

                case ColumnType.Double:
                    {
                        if (data.Double is not null)
                        {
                            return data.Double;
                        }

                        if (data.String is null)
                            throw new NullReferenceException();

                        return double.Parse(data.String);
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        private static bool IsBitSet(long num, int pos) => (num & (1 << pos)) != 0;
        private static int CountSetBits(long bitmask)
        {

            var count = 0;
            var mask = 1;
            for (var i = 0; i < 32; i++)
            {
                if ((mask & bitmask) == mask)
                    count++;
                mask <<= 1;
            }
            return count;
        }


    }
}
