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
                var headerLookup = result.Result.Data.Descriptor.Select.ToDictionary(x => x.Value);
                foreach (var dataSet in result.Result.Data.Dsr.DataSets)
                {
                    foreach (var (key, data) in dataSet.Ph.SelectMany(x => x))
                    {
                        await WriteCsv(name, auditBlobClient, processedLogClient, messageCollector, log, data, key, result, dataSet, headerLookup);
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

    private static async Task WriteCsv(string name, BlockBlobClient auditBlobClient, BlobContainerClient processedLogClient,
        IAsyncCollector<SendGridMessage> messageCollector, ILogger log, DataRow[] data, string key, ResultElement result, DataSet dataSet,
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

            var headers = data.Single(x => x.ColumnHeaders is not null).ColumnHeaders;

            WriteHeaders(headers, headerLookup, csvWriter);
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

    private static async Task<AuditLog> ReadJson(string name, BlockBlobClient auditBlobClient, ILogger log)
    {
        log.LogInformation("Processing {file}", name);
        var stream = await auditBlobClient.OpenReadAsync();
        using var streamReader = new StreamReader(stream, Encoding.UTF8);
        using JsonReader reader = new JsonTextReader(streamReader);

        var settings = new JsonSerializerSettings {
            MissingMemberHandling = MissingMemberHandling.Error
        };

        var serializer = JsonSerializer.Create(settings);

        return serializer.Deserialize<AuditLog>(reader);
    }

    private static void WriteHeaders(ColumnHeader[] headers, Dictionary<string, DescriptorSelect> headerLookup, CsvWriter csvWriter)
    {
        // Write CSV headers
        foreach (var headerName in headers.Select(x =>
        {
            return headerLookup[x.NameIndex].Kind switch {
                DescriptorKind.Select => string.Join("---", headerLookup[x.NameIndex].GroupKeys.Select(g => g.Source.Property)),
                DescriptorKind.Grouping => Regex.Replace(headerLookup[x.NameIndex].Name, @"^[^()]*\([^()]*\.([^().]*)\)[^()]*", "$1"),
                _ => throw new ArgumentOutOfRangeException(nameof(headerLookup))
            };
        }))
        {
            csvWriter.WriteField(headerName);
        }

        csvWriter.NextRecord();
    }

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

    private static object[] GetRow(DataRow row, ColumnHeader[] headers, Dictionary<string, string[]> valueDictionary, object[] previousCsvRow)
    {
        var rowDataIndex = 0;
        var csvRow = new object[headers.Length];

        var copyBitmask = row.CopyBitmask ?? 0;
        var nullBitmask = row.NullBitmask ?? 0;

        if (CountSetBits(copyBitmask) + CountSetBits(nullBitmask) + row.RowValues.Length != headers.Length)
            throw new ArgumentException($"Number of rows doesn't match the headers (rows: {CountSetBits(copyBitmask) + CountSetBits(nullBitmask) + row.RowValues.Length} headers:{headers.Length}");

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

            var data = row.RowValues[rowDataIndex++];
            switch (headers[index].ColumnType)
            {
                case ColumnType.String:
                    {
                        if (data.Integer is not null)
                        {
                            // need to lookup
                            var lookup = headers[index].DataIndex;
                            var value = valueDictionary[lookup][data.Integer.Value];
                            csvRow[index] = value;
                        }
                        else
                        {
                            csvRow[index] = data.String;
                        }
                        break;
                    }
                case ColumnType.Int:
                    {

                        if (data.Integer is null)
                            throw new NullReferenceException();

                        csvRow[index] = data.Integer;
                        break;
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        return csvRow;
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
