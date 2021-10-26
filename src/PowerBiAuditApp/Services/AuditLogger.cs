using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PowerBiAuditApp.Extensions;
using PowerBiAuditApp.Models;
using System.Text;

namespace PowerBiAuditApp.Services;

public class AuditLogger : IAuditLogger
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IOptions<StorageAccountSettings> _settings;

    public AuditLogger(BlobServiceClient blobServiceClient, IOptions<StorageAccountSettings> settings)
    {
        _blobServiceClient = blobServiceClient;
        _settings = settings;
    }

    /// <summary>
    /// Audit both user and query data returned.
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="responseMessage"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task CreateAuditLog(HttpContext httpContext, HttpResponseMessage responseMessage, CancellationToken cancellationToken = default)
    {
        if (!responseMessage.IsContentOfType("application/json") || !httpContext.Request.Path.ToString().Contains("querydata"))
            return;

        var stringContent = responseMessage.Content.ReadAsStringAsync(cancellationToken).Result;
        stringContent = System.Web.HttpUtility.UrlDecode(stringContent);


        httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(httpContext.Request.Body);
        var requestBody = await reader.ReadToEndAsync();

        // Do something
        var payload = new
        {
            User = httpContext.User.Identity?.Name,
            Request = JObject.Parse(requestBody),
            Response = JObject.Parse(stringContent)
        };

        var queueClient = await GetQueueClient(cancellationToken);
        var message = JsonConvert.SerializeObject(payload);

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(message));
        await queueClient.UploadBlobAsync($"{Guid.NewGuid()} {DateTime.UtcNow:yyyy-MM-dd hh-mm-ss}.json", stream, cancellationToken);

        if (_settings.Value.WriteFile)
        {
            var dt = "audit/" + DateTime.UtcNow.ToString("yyyyMMddhhmmss") + Guid.NewGuid() + ".json";
            Directory.CreateDirectory("audit");
            await using var writer = File.CreateText(dt);
            await writer.WriteAsync(message);
        }
    }

    private async Task<BlobContainerClient> GetQueueClient(CancellationToken cancellationToken)
    {
        var client = _blobServiceClient.GetBlobContainerClient(_settings.Value.AuditPreProcessBlobStorageName?.ToLower());
        await client.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        return client;
    }
}