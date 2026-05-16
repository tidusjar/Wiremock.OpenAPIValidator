using System.Net;
using Wiremock.OpenAPIValidator.Commands;

namespace Wiremock.OpenAPIValidator.Tests.Commands
{
    public class WiremockMappingsReaderCommandHandlerTests
    {
        private WiremockMappingsReaderCommandHandler _handler;
        private string _tempFilePath;

        [SetUp]
        public void Setup()
        {
            _handler = new WiremockMappingsReaderCommandHandler();
            _tempFilePath = Path.Combine(Path.GetTempPath(), $"wiremock-mappings-{Guid.NewGuid():N}.json");
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_tempFilePath))
            {
                File.Delete(_tempFilePath);
            }
        }

        [Test]
        public void Handle_FileDoesNotExist_ThrowsFileNotFoundException()
        {
            var command = new WiremockMappingsReaderCommand
            {
                WiremockMappingPath = Path.Combine(Path.GetTempPath(), $"does-not-exist-{Guid.NewGuid():N}.json")
            };

            Assert.That(
                async () => await _handler.Handle(command, CancellationToken.None),
                Throws.TypeOf<FileNotFoundException>());
        }

        [Test]
        public async Task Handle_MultiMappingFile_ReturnsAllMappings()
        {
            var json = """
            {
              "mappings": [
                {
                  "request": {
                    "method": "GET",
                    "urlPattern": "/api/v1/first"
                  },
                  "response": {
                    "status": 200,
                    "bodyFileName": "first.json"
                  }
                },
                {
                  "request": {
                    "method": "POST",
                    "urlPattern": "/api/v1/second"
                  },
                  "response": {
                    "status": 201,
                    "bodyFileName": "second.json"
                  }
                }
              ]
            }
            """;
            await File.WriteAllTextAsync(_tempFilePath, json);

            var result = await _handler.Handle(
                new WiremockMappingsReaderCommand { WiremockMappingPath = _tempFilePath },
                CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Mappings, Has.Count.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(result.Mappings[0].Request!.Method, Is.EqualTo("GET"));
                Assert.That(result.Mappings[0].Request!.UrlPattern, Is.EqualTo("/api/v1/first"));
                Assert.That(result.Mappings[0].Response!.Status, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(result.Mappings[0].Response!.FileName, Is.EqualTo("first.json"));
                Assert.That(result.Mappings[1].Request!.Method, Is.EqualTo("POST"));
                Assert.That(result.Mappings[1].Request!.UrlPattern, Is.EqualTo("/api/v1/second"));
                Assert.That(result.Mappings[1].Response!.Status, Is.EqualTo(HttpStatusCode.Created));
                Assert.That(result.Mappings[1].Response!.FileName, Is.EqualTo("second.json"));
            });
        }

        [Test]
        public async Task Handle_SingleMappingFile_ReturnsSingleMapping()
        {
            var json = """
            {
              "request": {
                "method": "GET",
                "urlPattern": "/api/v1/single"
              },
              "response": {
                "status": 200,
                "bodyFileName": "single.json"
              }
            }
            """;
            await File.WriteAllTextAsync(_tempFilePath, json);

            var result = await _handler.Handle(
                new WiremockMappingsReaderCommand { WiremockMappingPath = _tempFilePath },
                CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Mappings, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Mappings[0].Request, Is.Not.Null);
                Assert.That(result.Mappings[0].Request!.Method, Is.EqualTo("GET"));
                Assert.That(result.Mappings[0].Request!.UrlPattern, Is.EqualTo("/api/v1/single"));
                Assert.That(result.Mappings[0].Response, Is.Not.Null);
                Assert.That(result.Mappings[0].Response!.Status, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(result.Mappings[0].Response!.FileName, Is.EqualTo("single.json"));
            });
        }

        [Test]
        public async Task Handle_EmptyMappingsArray_ReturnsEmptyMappings()
        {
            var json = """{ "mappings": [] }""";
            await File.WriteAllTextAsync(_tempFilePath, json);

            var result = await _handler.Handle(
                new WiremockMappingsReaderCommand { WiremockMappingPath = _tempFilePath },
                CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Mappings, Is.Empty);
        }

        [Test]
        public async Task Handle_UnrecognizedJsonShape_ReturnsEmptyMappings()
        {
            var json = """{ "unexpected": "value" }""";
            await File.WriteAllTextAsync(_tempFilePath, json);

            var result = await _handler.Handle(
                new WiremockMappingsReaderCommand { WiremockMappingPath = _tempFilePath },
                CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Mappings, Is.Empty);
        }
    }
}
