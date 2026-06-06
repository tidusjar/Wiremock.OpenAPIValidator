using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;

namespace Wiremock.OpenAPIValidator.Tests
{
    public class ValidationServiceTests
    {
        private ValidationService _service;
        private string _rootPath;
        private string _mappingsPath;
        private string _filesPath;
        private string _specPath;

        [SetUp]
        public void Setup()
        {
            var provider = new ServiceCollection()
                .AddValidatorServices()
                .BuildServiceProvider();
            _service = provider.GetRequiredService<ValidationService>();

            // Standard WireMock layout: mappings/ and __files/ are siblings under a root.
            _rootPath = Path.Combine(Path.GetTempPath(), $"wiremock-validation-{Guid.NewGuid():N}");
            _mappingsPath = Path.Combine(_rootPath, "mappings");
            _filesPath = Path.Combine(_rootPath, "__files");
            Directory.CreateDirectory(_mappingsPath);
            Directory.CreateDirectory(_filesPath);
            _specPath = Path.Combine(_rootPath, "openapi.json");
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
        public async Task SuccessfulValidation()
        {
            WriteSpec(BuildSpec(
                "/api/v1/widgets",
                OperationType.Get,
                "getWidgets",
                new[] { QueryParam("id", required: true, format: "int32") },
                new[] { ("id", "integer", true), ("name", "string", true) }));

            WriteResponseBody("widget.json", """{ "id": 1, "name": "widget" }""");
            WriteMapping("widget.json", """
            {
              "request": {
                "method": "GET",
                "urlPattern": "/api/v1/widgets",
                "queryParameters": { "id": { "equalTo": "1" } }
              },
              "response": {
                "status": 200,
                "bodyFileName": "widget.json"
              }
            }
            """);

            var result = await _service.ValidateAsync(_specPath, _mappingsPath);

            Assert.That(result.Results, Is.Not.Empty);
            Assert.Multiple(() =>
            {
                Assert.That(result.Results.Select(r => r.ValidationResult),
                    Is.All.EqualTo(ValidationResult.Passed));
                // Pin the full set of checks that ran, so a silently-dropped node type is caught.
                Assert.That(result.Results.Select(r => r.Type).Distinct(), Is.EquivalentTo(new[]
                {
                    ValidatorType.UrlMatch,
                    ValidatorType.Method,
                    ValidatorType.ParamRequired,
                    ValidatorType.ParamType,
                    ValidatorType.ResponsePropertyRequired,
                    ValidatorType.ResponsePropertyType,
                }));
                Assert.That(result.Valid, Is.True);
            });
        }

        [Test]
        public async Task InlineJsonBody_Validates()
        {
            WriteSpec(BuildSpec(
                "/api/v1/widgets",
                OperationType.Get,
                "getWidgets",
                parameters: null,
                new[] { ("id", "integer", true), ("name", "string", true) }));

            WriteMapping("widget.json", """
            {
              "request": {
                "method": "GET",
                "urlPattern": "/api/v1/widgets"
              },
              "response": {
                "status": 200,
                "jsonBody": { "id": 1, "name": "widget" }
              }
            }
            """);

            var result = await _service.ValidateAsync(_specPath, _mappingsPath);

            Assert.Multiple(() =>
            {
                Assert.That(
                    result.Results.Any(r => r.Type == ValidatorType.ResponsePropertyType
                        && r.ValidationResult == ValidationResult.Passed),
                    Is.True);
                Assert.That(result.Valid, Is.True);
            });
        }

        [Test]
        public async Task UnmatchedPath_Fails()
        {
            WriteSpec(BuildSpec(
                "/api/v1/widgets",
                OperationType.Get,
                "getWidgets",
                parameters: null,
                new[] { ("id", "integer", true) }));

            WriteMapping("orphan.json", """
            {
              "request": {
                "method": "GET",
                "urlPattern": "/api/v1/nonexistent"
              },
              "response": {
                "status": 200,
                "jsonBody": { "id": 1 }
              }
            }
            """);

            var result = await _service.ValidateAsync(_specPath, _mappingsPath);

            Assert.Multiple(() =>
            {
                // The unmatched path is the only failure; nothing downstream should run.
                Assert.That(
                    result.Results.Where(r => r.ValidationResult == ValidationResult.Failed)
                        .Select(r => r.Type),
                    Is.EquivalentTo(new[] { ValidatorType.UrlMatch }));
                Assert.That(result.Valid, Is.False);
            });
        }

        [Test]
        public async Task MissingRequiredParam_Fails()
        {
            WriteSpec(BuildSpec(
                "/api/v1/widgets",
                OperationType.Get,
                "getWidgets",
                new[] { QueryParam("id", required: true, format: "int32") },
                new[] { ("id", "integer", true) }));

            WriteMapping("widget.json", """
            {
              "request": {
                "method": "GET",
                "urlPattern": "/api/v1/widgets"
              },
              "response": {
                "status": 200,
                "jsonBody": { "id": 1 }
              }
            }
            """);

            var result = await _service.ValidateAsync(_specPath, _mappingsPath);

            Assert.Multiple(() =>
            {
                // The absent required param is the only thing that fails (both param checks for it).
                Assert.That(
                    result.Results.Where(r => r.ValidationResult == ValidationResult.Failed)
                        .Select(r => r.Type),
                    Is.EquivalentTo(new[] { ValidatorType.ParamRequired, ValidatorType.ParamType }));
                Assert.That(result.Valid, Is.False);
            });
        }

        [Test]
        public async Task MissingRequiredResponseProperty_Fails()
        {
            WriteSpec(BuildSpec(
                "/api/v1/widgets",
                OperationType.Get,
                "getWidgets",
                parameters: null,
                new[] { ("id", "integer", true), ("name", "string", true) }));

            WriteMapping("widget.json", """
            {
              "request": {
                "method": "GET",
                "urlPattern": "/api/v1/widgets"
              },
              "response": {
                "status": 200,
                "jsonBody": { "id": 1 }
              }
            }
            """);

            var result = await _service.ValidateAsync(_specPath, _mappingsPath);

            Assert.Multiple(() =>
            {
                // The missing required property is the only failure.
                Assert.That(
                    result.Results.Where(r => r.ValidationResult == ValidationResult.Failed)
                        .Select(r => r.Type),
                    Is.EquivalentTo(new[] { ValidatorType.ResponsePropertyRequired }));
                Assert.That(result.Valid, Is.False);
            });
        }

        [Test]
        public async Task OptionalProperty_Warns()
        {
            WriteSpec(BuildSpec(
                "/api/v1/widgets",
                OperationType.Get,
                "getWidgets",
                parameters: null,
                new[] { ("id", "integer", true), ("description", "string", false) }));

            WriteMapping("widget.json", """
            {
              "request": {
                "method": "GET",
                "urlPattern": "/api/v1/widgets"
              },
              "response": {
                "status": 200,
                "jsonBody": { "id": 1 }
              }
            }
            """);

            var result = await _service.ValidateAsync(_specPath, _mappingsPath);

            Assert.Multiple(() =>
            {
                // An absent optional property warns rather than fails.
                Assert.That(
                    result.Results.Any(r => r.ValidationResult == ValidationResult.Failed),
                    Is.False);
                Assert.That(
                    result.Results.Any(r => r.Type == ValidatorType.ResponsePropertyRequired
                        && r.ValidationResult == ValidationResult.Warning),
                    Is.True);
                Assert.That(result.Valid, Is.False);
            });
        }

        // ---- helpers ----

        private void WriteSpec(OpenApiDocument document) =>
            File.WriteAllText(_specPath, document.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0));

        private void WriteMapping(string fileName, string json) =>
            File.WriteAllText(Path.Combine(_mappingsPath, fileName), json);

        private void WriteResponseBody(string fileName, string json) =>
            File.WriteAllText(Path.Combine(_filesPath, fileName), json);

        private static OpenApiParameter QueryParam(string name, bool required, string format) =>
            new()
            {
                Name = name,
                In = ParameterLocation.Query,
                Required = required,
                Schema = new OpenApiSchema { Type = "string", Format = format }
            };

        private static OpenApiDocument BuildSpec(
            string path,
            OperationType method,
            string operationId,
            IEnumerable<OpenApiParameter>? parameters,
            IEnumerable<(string Name, string Type, bool Required)> responseProperties)
        {
            var schema = new OpenApiSchema { Type = "object" };
            foreach (var property in responseProperties)
            {
                schema.Properties[property.Name] = new OpenApiSchema { Type = property.Type };
                if (property.Required)
                {
                    schema.Required.Add(property.Name);
                }
            }

            var operation = new OpenApiOperation
            {
                OperationId = operationId,
                Parameters = parameters?.ToList() ?? new List<OpenApiParameter>(),
                Responses = new OpenApiResponses
                {
                    ["200"] = new OpenApiResponse
                    {
                        Description = "OK",
                        Content =
                        {
                            ["application/json"] = new OpenApiMediaType { Schema = schema }
                        }
                    }
                }
            };

            return new OpenApiDocument
            {
                Info = new OpenApiInfo { Title = "Test API", Version = "1.0.0" },
                Paths = new OpenApiPaths
                {
                    [path] = new OpenApiPathItem
                    {
                        Operations = { [method] = operation }
                    }
                }
            };
        }
    }
}
