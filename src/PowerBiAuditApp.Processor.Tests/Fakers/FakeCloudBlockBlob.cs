using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace PowerBiAuditApp.Processor.Tests.Fakers
{
    internal class FakeCloudBlockBlob : CloudBlockBlob
    {
        private readonly Stream _stream;

        public FakeCloudBlockBlob(Stream stream) : base(new Uri("http://127.0.0.1:10002/devstoreaccount1/screenSettings"))
        {
            _stream = stream;
        }

        public override Task<CloudBlobStream> OpenWriteAsync()
        {
            if (_stream is CloudBlobStream cloudBlobStream)
                return Task.FromResult(cloudBlobStream);
            return Task.FromResult<CloudBlobStream>(new FakeCloudBlobStream(_stream));
        }

        public override Task<Stream> OpenReadAsync() => Task.FromResult(_stream);
        public override Task DeleteAsync() => Task.CompletedTask;
    }
}