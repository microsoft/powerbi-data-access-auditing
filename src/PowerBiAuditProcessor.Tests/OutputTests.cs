using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PowerBiAuditProcessor.Tests.Fakers;
using SendGrid.Helpers.Mail;
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


        await PowerBiAuditLogProcessor.Run(filePath.Replace(ExamplesFolder, ""), blob, container, new Mock<IAsyncCollector<SendGridMessage>>().Object, new Mock<ILogger>().Object);

        foreach (var (resultFileName, file) in container.GetFiles())
        {
            var settings = new VerifySettings();
            settings.UseDirectory("Results");
            settings.UseFileName(resultFileName);
            await Verifier.Verify(file, settings);
        }

    }

    [Theory]
    [MemberData(nameof(ExampleFiles))]
    public async Task InputHasAdditionalProperty_WhenTheProcessIsRun_AndErrorEmailIsSent(string filePath)
    {
        //Arrange
        var jsonString = await File.ReadAllTextAsync(filePath);
        var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);
        jObject!["AProperty"] = "An empty string";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(jObject.ToString()));
        var blob = new FakeBlockBlobClient(stream);
        var container = new FakeContainerClient();
        var collectorMock = new Mock<IAsyncCollector<SendGridMessage>>();

        // Act
        await PowerBiAuditLogProcessor.Run(filePath.Replace(ExamplesFolder, ""), blob, container, collectorMock.Object, new Mock<ILogger>().Object);


        //Assert
        collectorMock.Verify(x => x.AddAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }


    [Theory]
    [MemberData(nameof(ExampleFiles))]
    public async Task InputIsMissingAProperty_WhenTheProcessIsRun_AndErrorEmailIsSent(string filePath)
    {
        //Arrange
        var jsonString = await File.ReadAllTextAsync(filePath);
        var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);
        jObject!["Response"]!["results"]!.Parent!.Remove();

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(jObject.ToString()));
        var blob = new FakeBlockBlobClient(stream);
        var container = new FakeContainerClient();
        var collectorMock = new Mock<IAsyncCollector<SendGridMessage>>();

        // Act
        await PowerBiAuditLogProcessor.Run(filePath.Replace(ExamplesFolder, ""), blob, container, collectorMock.Object, new Mock<ILogger>().Object);


        //Assert
        collectorMock.Verify(x => x.AddAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

}