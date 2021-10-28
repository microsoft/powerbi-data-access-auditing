using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PowerBiAuditProcessor.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataRow = PowerBiAuditProcessor.Models.DataRow;
using DataSet = PowerBiAuditProcessor.Models.DataSet;

namespace PowerBiAuditProcessor;

public static class PowerBiAuditLogProcessor
{
    [FunctionName(nameof(PowerBiAuditLogProcessor))]
    public static async Task Run(
        [QueueTrigger("audit-log-queue", Connection = "AzureWebJobsStorage")] string name,
        [Blob("audit-pre-processed-log-storage/{queueTrigger}", FileAccess.Read, Connection = "AzureWebJobsStorage")] BlockBlobClient auditBlobClient,
        [Blob("audit-processed-log-storage", Connection = "AzureWebJobsStorage")] BlobContainerClient processedLogClient,
        ILogger log
        )
    {
        var model = await ReadJson(name, auditBlobClient, log);

        foreach (var result in model.Response.Results)
        {
            var headerLookup = result.Result.Data.Descriptor.Select.ToDictionary(x => x.Value);
            foreach (var dataSet in result.Result.Data.Dsr.DataSets)
            {
                foreach (var (key, data) in dataSet.Ph.SelectMany(x => x))
                {
                    // If no data was returned don't log it
                    if (!data.Any())
                    {
                        log.LogWarning("Did not find any data in {fileName}:{key}", name, key);
                        continue;
                    }

                    var filename = $"{name} ({result.JobId}-{dataSet.Name}-{key}).csv";
                    var blob = processedLogClient.GetBlockBlobClient(filename);

                    await using var stream = await blob.OpenWriteAsync(true);
                    await using var writer = new StreamWriter(stream);
                    await using var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture) { LeaveOpen = false, HasHeaderRecord = false });

                    var headers = data.Single(x => x.ColumnHeaders is not null).ColumnHeaders;

                    WriteHeaders(headers, headerLookup, csvWriter);
                    WriteRows(data, headers, dataSet, csvWriter);

                    await csvWriter.FlushAsync();
                }
            }
        }

        await auditBlobClient.DeleteAsync();
    }

    private static async Task<AuditLog> ReadJson(string name, BlockBlobClient auditBlobClient, ILogger log)
    {
        log.LogInformation("Processing {file}", name);
        var stream = await auditBlobClient.OpenReadAsync();
        using var streamReader = new StreamReader(stream, Encoding.UTF8);
        using JsonReader reader = new JsonTextReader(streamReader);
        var serializer = new JsonSerializer();
        return serializer.Deserialize<AuditLog>(reader);
    }


    private static void WriteHeaders(ColumnHeader[] headers, Dictionary<string, DescriptorSelect> headerLookup, CsvWriter csvWriter)
    {
        // Write CSV headers
        foreach (var headerName in headers.Select(x => headerLookup[x.NameIndex].Name))
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
            var bitmask = row.Bitmask ?? 0;
            previousCsvRow = GetRow(row, headers, dataSet.ValueDictionary, bitmask, previousCsvRow);
            foreach (var rowData in previousCsvRow)
            {
                csvWriter.WriteField(rowData);
            }

            csvWriter.NextRecord();
        }
    }

    private static object[] GetRow(DataRow row, ColumnHeader[] headers, Dictionary<string, string[]> valueDictionary, long bitmask, object[] previousCsvRow)
    {
        var rowDataIndex = 0;
        var csvRow = new object[headers.Length];
        bitmask = long.MaxValue ^ bitmask;
        for (var index = 0; index < headers.Length; index++)
        {
            if (!IsBitSet(bitmask, index))
            {
                // this is a duplicate
                csvRow[index] = previousCsvRow[index];
            }
            else
            {
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
        }

        return csvRow;
    }

    private static bool IsBitSet(long num, int pos)
    {
        return (num & (1 << pos)) != 0;
    }
}