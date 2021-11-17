using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Blob;

namespace PowerBiAuditApp.Processor.Tests.Fakers
{
    internal class FakeCloudBlobContainer : CloudBlobContainer, IDisposable
    {
        private readonly Dictionary<string, FakeCloudBlobStream> _streams = new();

        public FakeCloudBlobContainer() : base(new Uri("http://test"))
        {
        }

        public override CloudBlockBlob GetBlockBlobReference(string blobName)
        {
            var stream = new FakeCloudBlobStream();
            var client = new FakeCloudBlockBlob(stream);

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

        ~FakeCloudBlobContainer()
        {
            ReleaseUnmanagedResources();
        }
    }
}