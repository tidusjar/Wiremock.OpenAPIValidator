using Microsoft.OpenApi.Models;
using Wiremock.OpenAPIValidator.Models;
using Wiremock.OpenAPIValidator.Queries;

namespace Wiremock.OpenAPIValidator.Tests.Queries;

public class PropertyTypeQueryHandlerTests
{

    private PropertyTypeQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _handler = new PropertyTypeQueryHandler();
    }

    [Test]
    public async Task Handle_ValidMatchingRequiredProperties()
    {
        var wiremockProps = new WiremockResponseProperties();
        wiremockProps.Properties.Add("Prop1", typeof(string));

        OpenApiResponses apiResponse = RequiredOpenApiSchema();

        var response = await _handler.Handle(new PropertyTypeQuery
        {
            Responses = apiResponse,
            MockProperties = wiremockProps,
            Name = "UnitTest"
        }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Count.EqualTo(1));
            Assert.That(response[0].Type, Is.EqualTo(ValidatorType.ResponsePropertyType));
            Assert.That(response[0].Name, Is.EqualTo("Response - UnitTest - prop1"));
            Assert.That(response[0].ValidationResult, Is.EqualTo(ValidationResult.Passed));
            Assert.That(response[0].Description, Is.Null.Or.Empty);
        });
    }

    [Test]
    public async Task Handle_InvalidType()
    {
        var wiremockProps = new WiremockResponseProperties();
        wiremockProps.Properties.Add("Prop1", typeof(int));

        OpenApiResponses apiResponse = RequiredOpenApiSchema();

        var response = await _handler.Handle(new PropertyTypeQuery
        {
            Responses = apiResponse,
            MockProperties = wiremockProps,
            Name = "UnitTest"
        }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Count.EqualTo(1));
            Assert.That(response[0].Type, Is.EqualTo(ValidatorType.ResponsePropertyType));
            Assert.That(response[0].Name, Is.EqualTo("Response - UnitTest - prop1"));
            Assert.That(response[0].ValidationResult, Is.EqualTo(ValidationResult.Failed));
            Assert.That(response[0].Description, Is.Not.Null.And.Contains("prop1"));
        });
    }

    [Test]
    public async Task Handle_MissingProp()
    {
        var wiremockProps = new WiremockResponseProperties();
        wiremockProps.Properties.Add("Prop2", typeof(int));

        OpenApiResponses apiResponse = RequiredOpenApiSchema();

        var response = await _handler.Handle(new PropertyTypeQuery
        {
            Responses = apiResponse,
            MockProperties = wiremockProps,
            Name = "UnitTest"
        }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Count.EqualTo(1));
            Assert.That(response[0].Type, Is.EqualTo(ValidatorType.ResponsePropertyType));
            Assert.That(response[0].Name, Is.EqualTo("Response - UnitTest - prop1"));
            Assert.That(response[0].ValidationResult, Is.EqualTo(ValidationResult.Warning));
            Assert.That(response[0].Description, Is.Not.Null.And.Contains("prop1"));
        });
    }

    [Test]
    public async Task Handle_MissingPropPassedIn()
    {
        var wiremockProps = new WiremockResponseProperties();
        wiremockProps.Properties.Add("Prop2", typeof(int));

        var response = await _handler.Handle(new PropertyTypeQuery
        {
            Responses = null,
            MockProperties = wiremockProps,
            Name = "UnitTest"
        }, CancellationToken.None);

        Assert.That(response, Has.Count.EqualTo(0));
    }

    [Test]
    public async Task Handle_MissingPropPassedInMock()
    {
        OpenApiResponses apiResponse = RequiredOpenApiSchema();

        var response = await _handler.Handle(new PropertyTypeQuery
        {
            Responses = apiResponse,
            MockProperties = null,
            Name = "UnitTest"
        }, CancellationToken.None);

        Assert.That(response, Has.Count.EqualTo(0));
    }

    [TestCase("date-time")]
    [TestCase("uuid")]
    [TestCase("number")]
    [TestCase("integer")]
    public async Task Handle_MissingPropType(string type)
    {
        var wiremockProps = new WiremockResponseProperties();
        wiremockProps.Properties.Add("Prop1", typeof(Assert));

        OpenApiResponses apiResponse = RequiredOpenApiSchema(type);

        var response = await _handler.Handle(new PropertyTypeQuery
        {
            Responses = apiResponse,
            MockProperties = wiremockProps,
            Name = "UnitTest"
        }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Count.EqualTo(1));
            Assert.That(response[0].Type, Is.EqualTo(ValidatorType.ResponsePropertyType));
            Assert.That(response[0].Name, Is.EqualTo("Response - UnitTest - prop1"));
            Assert.That(response[0].ValidationResult, Is.EqualTo(ValidationResult.Failed));
            Assert.That(response[0].Description, Is.Not.Null.And.Contains("prop1"));
        });
    }

    private static OpenApiResponses RequiredOpenApiSchema(string type = "string")
    {
        return new OpenApiResponses
        {
            {"200", new OpenApiResponse
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    { "application/json", new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Required = new HashSet<string> { "prop1" },
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                {"prop1", new OpenApiSchema {
                                    Type = type
                                }}
                            }
                        }
                    } }
                }
            } }
        };
    }
}