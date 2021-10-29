using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using PowerBiAuditProcessor.Tests.Fakers;
using VerifyTests;
using VerifyXunit;
using Xunit;

namespace PowerBiAuditProcessor.Tests;

[UsesVerify]
public class OutputTests
{
    private const string ExamplesFolder = "../../../Examples/";


    public static TheoryData<string> ExampleFiles {
        get {
            var data = new TheoryData<string>();
            foreach (var file in Directory.GetFiles(ExamplesFolder))
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


        await PowerBiAuditLogProcessor.Run(filePath.Replace(ExamplesFolder, ""), blob, container, new Mock<ILogger>().Object);

        foreach (var (resultFileName, file) in container.GetFiles())
        {
            var settings = new VerifySettings();
            settings.UseDirectory("Results");
            settings.UseFileName(resultFileName);
            await Verifier.Verify(file, settings);
        }

    }

}