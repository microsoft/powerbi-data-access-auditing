using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace PowerBiProcessor.Tests.Fakers;

internal class FakeBlockBlobClient : BlockBlobClient
{
    private readonly Stream _stream;

    public FakeBlockBlobClient(Stream stream)
    {
        _stream = stream;
    }

    public override Task<Stream> OpenWriteAsync(bool overwrite, BlockBlobOpenWriteOptions? options = null, CancellationToken cancellationToken = new()) => Task.FromResult(_stream);

    // ReSharper disable once OptionalParameterHierarchyMismatch
    public override Task<Stream> OpenReadAsync(long position, int? bufferSize = default, BlobRequestConditions? conditions = default, CancellationToken cancellationToken = default) => Task.FromResult(_stream);

    public override Task<Response?> DeleteAsync(DeleteSnapshotsOption snapshotsOption = DeleteSnapshotsOption.None, BlobRequestConditions? conditions = null, CancellationToken cancellationToken = new()) =>
        Task.FromResult<Response?>(null);
}