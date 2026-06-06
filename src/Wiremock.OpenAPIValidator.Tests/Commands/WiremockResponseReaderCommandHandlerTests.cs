
using System.Text.Json.Nodes;
using Wiremock.OpenAPIValidator.Commands;
using Wiremock.OpenAPIValidator.Models;

namespace Wiremock.OpenAPIValidator.Tests.Commands
{
    public class WiremockResponseReaderCommandHandlerTests
    {
        private WiremockResponseReaderCommandHandler _handler;
        private string _rootPath;
        private string _mappingsPath;
        private string _filesPath;

        [SetUp]
        public void Setup()
        {
            _handler = new WiremockResponseReaderCommandHandler();

            // Standard WireMock layout: mappings/ and __files/ are siblings under a root.
            _rootPath = Path.Combine(Path.GetTempPath(), $"wiremock-response-{Guid.NewGuid():N}");
            _mappingsPath = Path.Combine(_rootPath, "mappings");
            _filesPath = Path.Combine(_rootPath, "__files");
            Directory.CreateDirectory(_mappingsPath);
            Directory.CreateDirectory(_filesPath);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_rootPath))
            {
                Directory.Delete(_rootPath, recursive: true);
            }
        }

        [Test]
        public async Task Handle_BadDirectory()
        {
            var response = await _handler.Handle(new WiremockResponseReaderCommand
            {
                WiremockMappingPath = string.Empty,
                WiremockResponse = new() { FileName = "response.json" }
            }, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Properties, Is.Empty);
            });
        }

        [Test]
        public async Task Handle_BadParentDirectory()
        {
            var response = await _handler.Handle(new WiremockResponseReaderCommand
            {
                WiremockMappingPath = Directory.GetDirectoryRoot(Directory.GetCurrentDirectory()),
                WiremockResponse = new() { FileName = "response.json" }
            }, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Properties, Is.Empty);
            });
        }

        [Test]
        public async Task Handle_MissingResponseBodyFile_ReturnsEmptyProperties()
        {
            var response = await _handler.Handle(new WiremockResponseReaderCommand
            {
                WiremockMappingPath = _mappingsPath,
                WiremockResponse = new() { FileName = "does-not-exist.json" }
            }, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Properties, Is.Empty);
            });
        }

        [Test]
        public async Task Handle_ObjectResponseBodyFile_ReturnsObjectProperties()
        {
            WiremockResponse mockResponse = new();
            var responseFileName = "object-response.json";
            var json = """
            {
              "id": 1,
              "name": "widget",
              "inStock": true
            }
            """;
            await File.WriteAllTextAsync(Path.Combine(_filesPath, responseFileName), json);
            mockResponse.FileName = responseFileName;

            var result = await _handler.Handle(new WiremockResponseReaderCommand
            {
                WiremockMappingPath = _mappingsPath,
                WiremockResponse = mockResponse
            }, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.ObjectType, Is.EqualTo(ObjectType.Object));
                Assert.That(result.Properties, Has.Count.EqualTo(3));
                Assert.That(result.Properties["id"], Is.EqualTo(typeof(int)));
                Assert.That(result.Properties["name"], Is.EqualTo(typeof(string)));
                Assert.That(result.Properties["inStock"], Is.EqualTo(typeof(bool)));
            });
        }

        [Test]
        public async Task Handle_ArrayResponseBodyFile_ReturnsArrayProperties()
        {
            WiremockResponse mockResponse = new();
            var responseFileName = "array-response.json";
            var json = """
            [
              {
                "id": 1,
                "name": "widget"
              }
            ]
            """;
            await File.WriteAllTextAsync(Path.Combine(_filesPath, responseFileName), json);

            mockResponse.FileName = responseFileName;

            var result = await _handler.Handle(new WiremockResponseReaderCommand
            {
                WiremockMappingPath = _mappingsPath,
                WiremockResponse = mockResponse
            }, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.ObjectType, Is.EqualTo(ObjectType.Array));
                Assert.That(result.Properties, Has.Count.EqualTo(2));
                Assert.That(result.Properties["id"], Is.EqualTo(typeof(int)));
                Assert.That(result.Properties["name"], Is.EqualTo(typeof(string)));
            });
        }

        [Test]
        public async Task Handle_InlineJsonBodyObject_ReturnsObjectProperties()
        {
            WiremockResponse mockResponse = new()
            {
                JsonBody = JsonNode.Parse("""
                {
                  "id": 1,
                  "name": "widget",
                  "inStock": true
                }
                """)
            };

            var result = await _handler.Handle(new WiremockResponseReaderCommand
            {
                WiremockMappingPath = _mappingsPath,
                WiremockResponse = mockResponse
            }, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.ObjectType, Is.EqualTo(ObjectType.Object));
                Assert.That(result.Properties, Has.Count.EqualTo(3));
                Assert.That(result.Properties["id"], Is.EqualTo(typeof(int)));
                Assert.That(result.Properties["name"], Is.EqualTo(typeof(string)));
                Assert.That(result.Properties["inStock"], Is.EqualTo(typeof(bool)));
            });
        }

        [Test]
        public async Task Handle_InlineJsonBodyArray_ReturnsArrayProperties()
        {
            WiremockResponse mockResponse = new()
            {
                JsonBody = JsonNode.Parse("""
                [
                  {
                    "id": 1,
                    "name": "widget"
                  }
                ]
                """)
            };

            var result = await _handler.Handle(new WiremockResponseReaderCommand
            {
                WiremockMappingPath = _mappingsPath,
                WiremockResponse = mockResponse
            }, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.ObjectType, Is.EqualTo(ObjectType.Array));
                Assert.That(result.Properties, Has.Count.EqualTo(2));
                Assert.That(result.Properties["id"], Is.EqualTo(typeof(int)));
                Assert.That(result.Properties["name"], Is.EqualTo(typeof(string)));
            });
        }

        [Test]
        public async Task Handle_InlineJsonBodyTakesPrecedenceOverFileName()
        {
            // The file would yield an array; the inline body yields an object.
            var responseFileName = "ignored-response.json";
            await File.WriteAllTextAsync(Path.Combine(_filesPath, responseFileName), """
            [
              {
                "ignored": 1
              }
            ]
            """);

            WiremockResponse mockResponse = new()
            {
                FileName = responseFileName,
                JsonBody = JsonNode.Parse("""
                {
                  "id": 1,
                  "name": "widget"
                }
                """)
            };

            var result = await _handler.Handle(new WiremockResponseReaderCommand
            {
                WiremockMappingPath = _mappingsPath,
                WiremockResponse = mockResponse
            }, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.ObjectType, Is.EqualTo(ObjectType.Object));
                Assert.That(result.Properties, Has.Count.EqualTo(2));
                Assert.That(result.Properties["id"], Is.EqualTo(typeof(int)));
                Assert.That(result.Properties["name"], Is.EqualTo(typeof(string)));
                Assert.That(result.Properties, Does.Not.ContainKey("ignored"));
            });
        }
    }
}
