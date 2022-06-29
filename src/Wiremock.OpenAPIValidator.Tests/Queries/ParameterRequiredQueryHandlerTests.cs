using Microsoft.OpenApi.Models;
using System.Text.Json;
using Wiremock.OpenAPIValidator.Queries;

namespace Wiremock.OpenAPIValidator.Tests.Queries;

public class ParameterRequiredQueryHandlerTests
{

    private ParameterRequiredQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _handler = new ParameterRequiredQueryHandler();
    }

    [Test]
    public async Task Handle_RequiredMissingParam()
    {
        var mockedParam = "{ \"Param2\": { \"equalTo\": \"All\" } }";
        var doc = JsonDocument.Parse(mockedParam);

        var response = await _handler.Handle(new ParameterRequiredQuery
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
            Assert.That(response.Type, Is.EqualTo(ValidatorType.ParamRequired));
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

        var response = await _handler.Handle(new ParameterRequiredQuery
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
            Assert.That(response.Type, Is.EqualTo(ValidatorType.ParamRequired));
            Assert.That(response.Name, Is.EqualTo("UnitTest"));
            Assert.That(response.ValidationResult, Is.EqualTo(ValidationResult.Warning));
            Assert.That(response.Description, Is.Not.Null.And.Contains("Param1"));
        });
    }


    [Test]
    public async Task Handle_NullParam()
    {
        var mockedParam = "{ \"Param2\": { \"equalTo\": \"All\" } }";
        var doc = JsonDocument.Parse(mockedParam);

        var response = await _handler.Handle(new ParameterRequiredQuery
        {
            Name = "UnitTest",
            Param = null,
            MockedParameters = doc.RootElement
        }, CancellationToken.None);
        Assert.That(response, Is.InstanceOf<ValidatorNode?>());
    }

    [Test]
    public async Task Handle_CorrectParam([Values] bool required)
    {
        var mockedParam = "{ \"Param1\": { \"equalTo\": \"All\" } }";
        var doc = JsonDocument.Parse(mockedParam);

        var response = await _handler.Handle(new ParameterRequiredQuery
        {
            Name = "UnitTest",
            Param = new OpenApiParameter
            {
                Name = "Param1",
                Required = required
            },
            MockedParameters = doc.RootElement
        }, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(response.Type, Is.EqualTo(ValidatorType.ParamRequired));
            Assert.That(response.Name, Is.EqualTo("UnitTest"));
            Assert.That(response.ValidationResult, Is.EqualTo(ValidationResult.Passed));
            Assert.That(response.Description, Is.Null.Or.Empty);
        });
    }
}