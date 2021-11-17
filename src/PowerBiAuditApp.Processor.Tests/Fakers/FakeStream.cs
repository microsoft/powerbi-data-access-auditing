using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace PowerBiAuditApp.Processor.Tests.Fakers
{
    internal class FakeCloudBlobStream : CloudBlobStream
    {
        private readonly MemoryStream _stream;
        private string _streamValue;
        public string Value => _streamValue ?? Encoding.UTF8.GetString(_stream.ToArray());

        public FakeCloudBlobStream(Stream stream)
        {
            if (stream is MemoryStream memoryStream)
            {
                _stream = memoryStream;
            }
            _stream = new MemoryStream();
            stream.CopyTo(_stream);
            _stream.Seek(0, SeekOrigin.Begin);
        }

        public FakeCloudBlobStream()
        {
            _stream = new MemoryStream();
        }

        public override void Flush() => _stream.Flush();

        public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);

        public override void SetLength(long value) => _stream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);

        public override bool CanRead => _stream.CanRead;
        public override bool CanSeek => _stream.CanSeek;
        public override bool CanWrite => _stream.CanWrite;
        public override long Length => _stream.Length;
        public override long Position { get => _stream.Position; set => _stream.Position = value; }
        public override Task CommitAsync()
        {
            _streamValue = Encoding.UTF8.GetString(_stream.ToArray());
            return Task.CompletedTask;
        }
    }
}