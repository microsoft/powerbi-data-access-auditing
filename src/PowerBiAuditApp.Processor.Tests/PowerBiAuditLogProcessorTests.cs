using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PowerBiAuditApp.Processor.Models;
using PowerBiAuditApp.Processor.Tests.Fakers;
using SendGrid.Helpers.Mail;
using VerifyTests;
using VerifyXunit;
using Xunit;

namespace PowerBiAuditApp.Processor.Tests
{
    [UsesVerify]
    public class PowerBiAuditLogProcessorTests
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
            var blob = new FakeCloudBlockBlob(stream);
            var container = new FakeCloudBlobContainer();

            var collectorMock = new Mock<IAsyncCollector<SendGridMessage>>();
            var messages = new List<string>();
            collectorMock.Setup(x => x.AddAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
                .Callback<SendGridMessage, CancellationToken>((message, _) => messages.Add(message.PlainTextContent));


            await PowerBiAuditLogProcessor.Run(filePath.Replace(ExamplesFolder, ""), blob, container, collectorMock.Object, new Mock<ILogger>().Object);

            var files = container.GetFiles();
            collectorMock.Verify(x => x.AddAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Never, $"Messages sent: {JsonConvert.SerializeObject(messages)}");
            Assert.NotEmpty(files);

            foreach (var (resultFileName, file) in files)
            {
                var settings = new VerifySettings();
                settings.UseDirectory("Results");
                settings.UseFileName(resultFileName.Replace(".csv", ""));
                settings.UseExtension("csv");
                //settings.AutoVerify();
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
            var blob = new FakeCloudBlockBlob(stream);
            var container = new FakeCloudBlobContainer();
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
            var blob = new FakeCloudBlockBlob(stream);
            var container = new FakeCloudBlobContainer();
            var collectorMock = new Mock<IAsyncCollector<SendGridMessage>>();

            // Act
            await PowerBiAuditLogProcessor.Run(filePath.Replace(ExamplesFolder, ""), blob, container, collectorMock.Object, new Mock<ILogger>().Object);


            //Assert
            collectorMock.Verify(x => x.AddAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }


        [Theory]
        [MemberData(nameof(ExampleFiles))]
        public async Task InputHeadersHaveLessRowsThanData_WhenTheProcessIsRun_AndErrorEmailIsSent(string filePath)
        {
            //Arrange
            var jsonString = await File.ReadAllTextAsync(filePath);
            var model = JsonConvert.DeserializeObject<AuditLog>(jsonString);
            var headerData = model!.Response.Results.Single().Result.Data.Dsr.DataOrRow.Single().PrimaryRows.Single().Values.Single().Single(x => x.ColumnHeaders is not null);
            headerData.ColumnHeaders = headerData.ColumnHeaders.Take(headerData.ColumnHeaders.Length - 1).ToArray();

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model)));
            var blob = new FakeCloudBlockBlob(stream);
            var container = new FakeCloudBlobContainer();
            var collectorMock = new Mock<IAsyncCollector<SendGridMessage>>();

            // Act
            await PowerBiAuditLogProcessor.Run(filePath.Replace(ExamplesFolder, ""), blob, container, collectorMock.Object, new Mock<ILogger>().Object);


            //Assert
            collectorMock.Verify(x => x.AddAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }


        [Theory]
        [MemberData(nameof(ExampleFiles))]
        public async Task InputHeadersHaveMoreRowsThanData_WhenTheProcessIsRun_AndErrorEmailIsSent(string filePath)
        {
            //Arrange
            var jsonString = await File.ReadAllTextAsync(filePath);
            var model = JsonConvert.DeserializeObject<AuditLog>(jsonString);
            var headerData = model!.Response.Results.Single().Result.Data.Dsr.DataOrRow.Single().PrimaryRows.Single().Values.Single().Single(x => x.ColumnHeaders is not null);
            headerData.ColumnHeaders = headerData.ColumnHeaders.Concat(headerData.ColumnHeaders.Take(1)).ToArray();

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model)));
            var blob = new FakeCloudBlockBlob(stream);
            var container = new FakeCloudBlobContainer();
            var collectorMock = new Mock<IAsyncCollector<SendGridMessage>>();

            // Act
            await PowerBiAuditLogProcessor.Run(filePath.Replace(ExamplesFolder, ""), blob, container, collectorMock.Object, new Mock<ILogger>().Object);


            //Assert
            collectorMock.Verify(x => x.AddAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }

    }
}