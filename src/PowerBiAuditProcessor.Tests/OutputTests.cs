using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VerifyTests;
using VerifyXunit;
using Xunit;

namespace PowerBiAuditProcessor.Tests;

[UsesVerify]
public class OutputTests
{
    private static readonly string _examplesFolder = "../../../Examples/";


    public static TheoryData<string> ExampleFiles
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (var file in Directory.GetFiles(_examplesFolder))
            {
                data.Add(file);
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(ExampleFiles))]
    public async Task ResultsMatchExpected(string filePath)
    {
        var stream = File.OpenRead(filePath);
        var blob = new FakeBlockBlobClient(stream);
        var container = new FakeContainerClient();


        await PowerBiAuditLogProcessor.Run(filePath.Replace(_examplesFolder, ""), blob, container, new Mock<ILogger>().Object);

        foreach (var (resultFileName, file) in container.GetFiles())
        {
            var settings = new VerifySettings();
            settings.UseDirectory("Results");
            settings.UseFileName(resultFileName);
            await Verifier.Verify(file, settings);
        }

    }

}


class FakeContainerClient : BlobContainerClient, IDisposable
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

    public Dictionary<string, string> GetFiles()
    {
        return _streams.ToDictionary(x => x.Key, x => x.Value.Value);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~FakeContainerClient() => ReleaseUnmanagedResources();
}

class FakeBlockBlobClient : BlockBlobClient
{
    private readonly Stream _stream;

    public FakeBlockBlobClient(Stream stream)
    {
        _stream = stream;
    }

    public override Task<Stream> OpenWriteAsync(bool overwrite, BlockBlobOpenWriteOptions? options = null, CancellationToken cancellationToken = new())
    {
        return Task.FromResult(_stream);
    }

    public override Task<Stream> OpenReadAsync(long position, int? bufferSize = default, BlobRequestConditions conditions = default, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_stream);
    }

    public override Task<Response?> DeleteAsync(DeleteSnapshotsOption snapshotsOption = DeleteSnapshotsOption.None, BlobRequestConditions conditions = null,
        CancellationToken cancellationToken = new())
    {
        return Task.FromResult<Response?>(null);
    }
}

class FakeStream : MemoryStream
{
    private string? _streamValue;

    public string Value => _streamValue ?? Encoding.UTF8.GetString(ToArray());

    public override void Close()
    {
        _streamValue = Encoding.UTF8.GetString(ToArray());
        base.Close();
    }
}