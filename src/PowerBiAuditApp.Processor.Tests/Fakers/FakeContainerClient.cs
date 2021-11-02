using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;

namespace PowerBiAuditApp.Processor.Tests.Fakers;

internal class FakeContainerClient : BlobContainerClient, IDisposable
{
    private readonly Dictionary<string, FakeStream> _streams = new();

    protected override BlockBlobClient GetBlockBlobClientCore(string blobName)
    {
        var stream = new FakeStream();
        var client = new FakeBlockBlobClient(stream);

        _streams.Add(blobName, stream);
        return client;
    }

    private void ReleaseUnmanagedResources()
    {
        // TODO release unmanaged resources here
        foreach (var stream in _streams.Values)
        {
            stream.Dispose();
        }
    }

    public Dictionary<string, string> GetFiles() => _streams.ToDictionary(x => x.Key, x => x.Value.Value);

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~FakeContainerClient()
    {
        ReleaseUnmanagedResources();
    }
}