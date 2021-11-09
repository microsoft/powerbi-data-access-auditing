using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PowerBiAuditApp.Processor.Extensions;
using PowerBiAuditApp.Processor.Models;
using SendGrid.Helpers.Mail;
using DataRow = PowerBiAuditApp.Processor.Models.DataRow;
using DataSet = PowerBiAuditApp.Processor.Models.DataSet;

namespace PowerBiAuditApp.Processor;

public static class PowerBiAuditLogProcessor
{
    [FunctionName(nameof(PowerBiAuditLogProcessor))]
    public static async Task Run(
        [QueueTrigger("audit-log-queue", Connection = "StorageAccountQueueEndpoint")] string name,
        [Blob("audit-pre-processed-log-storage/{queueTrigger}", FileAccess.Read, Connection = "StorageAccountBlobEndpoint")] BlockBlobClient auditBlobClient,
        [Blob("audit-processed-log-storage", Connection = "StorageAccountBlobEndpoint")] BlobContainerClient processedLogClient,
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
                var headerLookup = result.Result.Data.Descriptor.Select
                    .Where(x => x is not null)
                    .ToDictionary(x => x.Value)
                    .Concat(result.Result.Data.Descriptor.Select.Where(x => x?.Highlight?.Value is not null).ToDictionary(x => x.Highlight.Value)) // sometimes the lookup is based on the selection
                    .ToDictionary(x => x.Key, x => x.Value);

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




    /// <summary>
    /// Deserialize the json file
    /// </summary>
    private static async Task<AuditLog> ReadJson(string name, BlockBlobClient auditBlobClient, ILogger log)
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
    /// Process the returned json and write it to a csv file
    /// </summary>
    private static async Task WriteCsv(string name, BlockBlobClient auditBlobClient, BlobContainerClient processedLogClient,
        IAsyncCollector<SendGridMessage> messageCollector, ILogger log, DataRow[] data, Query[] queries, string key, ResultElement result, DataSet dataSet,
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
            var blob = processedLogClient.GetBlockBlobClient(filename);

            await using var stream = await blob.OpenWriteAsync(true);
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
            await FailWithError(messageCollector, $"Writing CSC data failed for: {name} ({result.JobId}-{dataSet.Name}-{key})", auditBlobClient, exception, log);
        }
    }

    /// <summary>
    /// Gets header records
    /// </summary>
    private static ColumnHeader[] GetHeaders(DataRow[] data)
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
            var subColumnCount = subDataRows.Count(SubDataRowHasValue);

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

    private static async Task FailWithError(IAsyncCollector<SendGridMessage> messageCollector, string message, BlockBlobClient auditBlobClient, Exception exception, ILogger log)
    {
        var sendGridMessage = new SendGridMessage {
            From = new EmailAddress(Environment.GetEnvironmentVariable("ErrorFromEmailAddress", EnvironmentVariableTarget.Process)),
            Subject = message,
            PlainTextContent = $"{message}\r\n in file {auditBlobClient.Uri}\r\n Please investigate."
        };
        sendGridMessage.AddTo(new EmailAddress(Environment.GetEnvironmentVariable("ErrorToEmailAddress", EnvironmentVariableTarget.Process)));


        await messageCollector.AddAsync(sendGridMessage);
        await messageCollector.FlushAsync();
        log.LogError(exception, message);
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
                if (headerDescriptor.Name.StartsWith("select"))
                {
                    var select = queries
                        .SelectMany(q => q.QueryQuery.Commands.Select(c =>
                            c.SemanticQueryDataShapeCommand.Query.Select.SingleOrDefault(s =>
                                s.Name == headerDescriptor.Name)))
                        .SingleOrDefault(q => q is not null);
                    if (select is not null)
                    {
                        return select.Measure.Property + suffix;
                    }
                }

                return Regex.Replace(headerDescriptor.Name, @"^[^()]*\([^()]*\.([^().]*)\)[^()]*", "$1") + suffix;
            default:
                throw new ArgumentOutOfRangeException(nameof(headerLookup));
        }
    }


    /// <summary>
    /// Write rows to csv
    /// </summary>
    private static void WriteRows(DataRow[] data, ColumnHeader[] headers, DataSet dataSet, CsvWriter csvWriter)
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
    private static object[] GetRow(DataRow row, ColumnHeader[] headers, Dictionary<string, string[]> valueDictionary, object[] previousCsvRow)
    {
        var rowDataIndex = 0;
        var csvRow = new object[headers.Length];

        var copyBitmask = row.CopyBitmask ?? 0;
        var nullBitmask = row.NullBitmask ?? 0;
        var subRowDataIndexLookup = new Dictionary<int, int>();

        var totalRows = CountSetBits(copyBitmask) +
                        CountSetBits(nullBitmask) +
                        row.RowValues.Length +
                        row.ValueLookup.Count +
                        (row.SubDataRows?.Sum(x =>
                            CountSetBits(x.CopyBitmask ?? 0) +
                            CountSetBits(x.NullBitmask ?? 0) +
                            x.RowValues.Length +
                            (x.ValueLookup.Any() ? 1 : 0)) ?? 0);

        if (totalRows != headers.Length)
            throw new ArgumentException($"Number of rows doesn't match the headers (rows: {totalRows} headers:{headers.Length}");

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

                var subData = row.SubDataRows.Where(SubDataRowHasValue).Where((_, i) => i == rowIndex).Single();

                if (IsBitSet(subData.CopyBitmask ?? 0, columnIndex))
                {
                    // this is a duplicate

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


    private static bool SubDataRowHasValue(SubDataRow subDataRow) =>
        subDataRow.CopyBitmask is not null ||
        subDataRow.NullBitmask is not null ||
        subDataRow.RowValues.Length > 0 ||
        subDataRow.ValueLookup.Any();

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
