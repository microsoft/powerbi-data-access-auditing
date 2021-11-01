using System.IO;
using System.Text;

namespace PowerBiAuditApp.Processor.Tests.Fakers;

internal class FakeStream : MemoryStream
{
    private string? _streamValue;

    public string Value => _streamValue ?? Encoding.UTF8.GetString(ToArray());

    public override void Close()
    {
        _streamValue = Encoding.UTF8.GetString(ToArray());
        base.Close();
    }
}