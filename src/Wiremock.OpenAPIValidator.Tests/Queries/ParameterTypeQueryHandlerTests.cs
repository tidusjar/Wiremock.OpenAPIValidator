using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using Wiremock.OpenAPIValidator.Queries;

namespace Wiremock.OpenAPIValidator.Tests.Queries;

public class ParameterTypeQueryHandlerTests
{

    private ParameterTypeQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _handler = new ParameterTypeQueryHandler();
    }

    [Test]
    public async Task Handle_RequiredMissingParam()
    {
        var mockedParam = "{ \"Param2\": { \"equalTo\": \"All\" } }";
        var doc = JsonDocument.Parse(mockedParam);

        var response = await _handler.Handle(new ParameterTypeQuery
        {
            Name = "UnitTest",
            Param = new OpenApiParameter
            {
                Name = "Param1",
                Required = true
            },
            MockedParameters = doc.RootElement
        }, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(response.Type, Is.EqualTo(ValidatorType.ParamType));
            Assert.That(response.Name, Is.EqualTo("UnitTest"));
            Assert.That(response.ValidationResult, Is.EqualTo(ValidationResult.Failed));
            Assert.That(response.Description, Is.Not.Null.And.Contains("Param1"));
        });
    }

    [Test]
    public async Task Handle_OptionalMissingParam()
    {
        var mockedParam = "{ \"Param2\": { \"equalTo\": \"All\" } }";
        var doc = JsonDocument.Parse(mockedParam);

        var response = await _handler.Handle(new ParameterTypeQuery
        {
            Name = "UnitTest",
            Param = new OpenApiParameter
            {
                Name = "Param1",
                Required = false
            },
            MockedParameters = doc.RootElement
        }, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(response.Type, Is.EqualTo(ValidatorType.ParamType));
            Assert.That(response.Name, Is.EqualTo("UnitTest"));
            Assert.That(response.ValidationResult, Is.EqualTo(ValidationResult.Warning));
            Assert.That(response.Description, Is.Not.Null.And.Contains("Param1"));
        });
    }

    [Test]
    public async Task Handle_RequiredParamCorrectTypeEnum()
    {
        var mockedParam = "{ \"Param1\": { \"equalTo\": \"All\" } }";
        var doc = JsonDocument.Parse(mockedParam);

        var response = await _handler.Handle(new ParameterTypeQuery
        {
            Name = "UnitTest",
            Param = new OpenApiParameter
            {
                Name = "Param1",
                Schema = new OpenApiSchema
                {
                    Enum = new List<IOpenApiAny> { new OpenApiString("All"), new OpenApiString("None") }
                },
                Required = false
            },
            MockedParameters = doc.RootElement
        }, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(response.Type, Is.EqualTo(ValidatorType.ParamType));
            Assert.That(response.Name, Is.EqualTo("UnitTest"));
            Assert.That(response.ValidationResult, Is.EqualTo(ValidationResult.Passed));
            Assert.That(response.Description, Is.Null.Or.Empty);
        });
    }

    [Test]
    public async Task Handle_RequiredParamIncorrectTypeEnum()
    {
        var mockedParam = "{ \"Param1\": { \"equalTo\": \"AAAAA\" } }";
        var doc = JsonDocument.Parse(mockedParam);

        var response = await _handler.Handle(new ParameterTypeQuery
        {
            Name = "UnitTest",
            Param = new OpenApiParameter
            {
                Name = "Param1",
                Schema = new OpenApiSchema
                {
                    Enum = new List<IOpenApiAny> { new OpenApiString("All"), new OpenApiString("None") }
                },
                Required = false
            },
            MockedParameters = doc.RootElement
        }, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(response.Type, Is.EqualTo(ValidatorType.ParamType));
            Assert.That(response.Name, Is.EqualTo("UnitTest"));
            Assert.That(response.ValidationResult, Is.EqualTo(ValidationResult.Failed));
            Assert.That(response.Description, Is.Not.Null.And.Contains("Param1").And.Contains("AAAAA"));
        });
    }

    [Test]
    public async Task Handle_RequiredParamEqualsToCorrectTypeDateTime()
    {
        var mockedParam = "{ \"Param1\": { \"equalTo\": \"2022-03-18T00:00:00.0000000\" } }";
        var doc = JsonDocument.Parse(mockedParam);

        var response = await _handler.Handle(new ParameterTypeQuery
        {
            Name = "UnitTest",
            Param = new OpenApiParameter
            {
                Name = "Param1",
                Schema = new OpenApiSchema
                {
                    Format = "date-time"
                },
                Required = false
            },
            MockedParameters = doc.RootElement
        }, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(response.Type, Is.EqualTo(ValidatorType.ParamType));
            Assert.That(response.Name, Is.EqualTo("UnitTest"));
            Assert.That(response.ValidationResult, Is.EqualTo(ValidationResult.Passed));
            Assert.That(response.Description, Is.Null.Or.Empty);
        });
    }

    [Test]
    public async Task Handle_RequiredParamMatchesCorrectTypeUuid()
    {
        var mockedParam = "{ \"Param1\": { \"matches\": \"^[{]?[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}[}]?$\" } }";
        var doc = JsonDocument.Parse(mockedParam);

        var response = await _handler.Handle(new ParameterTypeQuery
        {
            Name = "UnitTest",
            Param = new OpenApiParameter
            {
                Name = "Param1",
                Schema = new OpenApiSchema
                {
                    Format = "uuid"
                },
                Required = false
            },
            MockedParameters = doc.RootElement
        }, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(response.Type, Is.EqualTo(ValidatorType.ParamType));
            Assert.That(response.Name, Is.EqualTo("UnitTest"));
            Assert.That(response.ValidationResult, Is.EqualTo(ValidationResult.Passed));
            Assert.That(response.Description, Is.Null.Or.Empty);
        });
    }

    [Test]
    public async Task Handle_RequiredParamMatchesIncorrectTypeUuid()
    {
        var mockedParam = "{ \"Param1\": { \"matches\": \"^[{]?-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}[}]?$\" } }";
        var doc = JsonDocument.Parse(mockedParam);

        var response = await _handler.Handle(new ParameterTypeQuery
        {
            Name = "UnitTest",
            Param = new OpenApiParameter
            {
                Name = "Param1",
                Schema = new OpenApiSchema
                {
                    Format = "uuid"
                },
                Required = false
            },
            MockedParameters = doc.RootElement
        }, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(response.Type, Is.EqualTo(ValidatorType.ParamType));
            Assert.That(response.Name, Is.EqualTo("UnitTest"));
            Assert.That(response.ValidationResult, Is.EqualTo(ValidationResult.Failed));
            Assert.That(response.Description, Is.Not.Null.Or.Empty);
        });
    }

    [TestCase("date-time")]
    [TestCase("int32")]
    [TestCase("string")]
    [TestCase("int64")]
    public async Task Handle_RequiredParamMatchesNotSupportedType(string format)
    {
        var mockedParam = "{ \"Param1\": { \"matches\": \"^[{]?[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}[}]?$\" } }";
        var doc = JsonDocument.Parse(mockedParam);

        var response = await _handler.Handle(new ParameterTypeQuery
        {
            Name = "UnitTest",
            Param = new OpenApiParameter
            {
                Name = "Param1",
                Schema = new OpenApiSchema
                {
                    Format = format
                },
                Required = false
            },
            MockedParameters = doc.RootElement
        }, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(response.Type, Is.EqualTo(ValidatorType.ParamType));
            Assert.That(response.Name, Is.EqualTo("UnitTest"));
            Assert.That(response.ValidationResult, Is.EqualTo(ValidationResult.Warning));
            Assert.That(response.Description, Is.Not.Null.And.Contains("not yet supported"));
        });
    }

    [TestCase("uuid")]
    [TestCase("int32")]
    [TestCase("int64")]
    public async Task Handle_RequiredParamEqualsToIncorrectType(string format)
    {
        var mockedParam = "{ \"Param1\": { \"equalTo\": \"2022-03-18T00:00:00.0000000\" } }";
        var doc = JsonDocument.Parse(mockedParam);

        var response = await _handler.Handle(new ParameterTypeQuery
        {
            Name = "UnitTest",
            Param = new OpenApiParameter
            {
                Name = "Param1",
                Schema = new OpenApiSchema
                {
                    Format = format
                },
                Required = false
            },
            MockedParameters = doc.RootElement
        }, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(response.Type, Is.EqualTo(ValidatorType.ParamType));
            Assert.That(response.Name, Is.EqualTo("UnitTest"));
            Assert.That(response.ValidationResult, Is.EqualTo(ValidationResult.Failed));
            Assert.That(response.Description, Is.Not.Null.And.Contains("Param1"));
        });
    }

    [Test]
    public async Task Handle_NullParam()
    {
        var mockedParam = "{ \"Param1\": { \"equalTo\": \"2022-03-18T00:00:00.0000000\" } }";
        var doc = JsonDocument.Parse(mockedParam);

        var response = await _handler.Handle(new ParameterTypeQuery
        {
            Name = "UnitTest",
            Param = null,
            MockedParameters = doc.RootElement
        }, CancellationToken.None);
        Assert.That(response, Is.InstanceOf<ValidatorNode?>());
    }
}