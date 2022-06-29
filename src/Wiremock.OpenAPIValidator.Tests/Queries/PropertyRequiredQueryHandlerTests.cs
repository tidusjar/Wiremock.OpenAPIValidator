using Microsoft.OpenApi.Models;
using Wiremock.OpenAPIValidator.Models;
using Wiremock.OpenAPIValidator.Queries;

namespace Wiremock.OpenAPIValidator.Tests.Queries;

public class PropertyRequiredQueryHandlerTests
{

    private PropertyRequiredQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _handler = new PropertyRequiredQueryHandler();
    }

    [Test]
    public async Task Handle_ValidMatchingRequiredProperties()
    {
        var wiremockProps = new WiremockResponseProperties();
        wiremockProps.Properties.Add("Prop1", typeof(string));

        OpenApiResponses apiResponse = RequiredOpenApiSchema();

        var response = await _handler.Handle(new PropertyRequiredQuery
        {
            Responses = apiResponse,
            MockProperties = wiremockProps,
            Name = "UnitTest"
        }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Count.EqualTo(1));
            Assert.That(response[0].Type, Is.EqualTo(ValidatorType.ResponsePropertyRequired));
            Assert.That(response[0].Name, Is.EqualTo("Response - UnitTest - prop1"));
            Assert.That(response[0].ValidationResult, Is.EqualTo(ValidationResult.Passed));
            Assert.That(response[0].Description, Is.Null.Or.Empty);
        });
    }

    [Test]
    public async Task Handle_MissingRequiredProperties()
    {
        var wiremockProps = new WiremockResponseProperties();
        wiremockProps.Properties.Add("Prop2", typeof(string));

        OpenApiResponses apiResponse = RequiredOpenApiSchema();

        var response = await _handler.Handle(new PropertyRequiredQuery
        {
            Responses = apiResponse,
            MockProperties = wiremockProps,
            Name = "UnitTest"
        }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Count.EqualTo(1));
            Assert.That(response[0].Type, Is.EqualTo(ValidatorType.ResponsePropertyRequired));
            Assert.That(response[0].Name, Is.EqualTo("Response - UnitTest - prop1"));
            Assert.That(response[0].ValidationResult, Is.EqualTo(ValidationResult.Failed));
            Assert.That(response[0].Description, Is.Not.Null.And.Contains("prop1"));
        });
    }

    [Test]
    public async Task Handle_NullResponse()
    {
        var wiremockProps = new WiremockResponseProperties();
        wiremockProps.Properties.Add("Prop2", typeof(string));

        var response = await _handler.Handle(new PropertyRequiredQuery
        {
            Responses = null,
            MockProperties = wiremockProps,
            Name = "UnitTest"
        }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Count.EqualTo(0));
        });
    }
    [Test]
    public async Task Handle_NullProperties()
    {
        OpenApiResponses apiResponse = RequiredOpenApiSchema();

        var response = await _handler.Handle(new PropertyRequiredQuery
        {
            Responses = apiResponse,
            MockProperties = null,
            Name = "UnitTest"
        }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Count.EqualTo(0));
        });
    }

    [Test]
    public async Task Handle_MissingOptionalProperties()
    {
        var wiremockProps = new WiremockResponseProperties();
        wiremockProps.Properties.Add("Prop2", typeof(string));

        OpenApiResponses apiResponse = RequiredOpenApiSchema();
        apiResponse["200"].Content["application/json"].Schema.Required = new HashSet<string>();

        var response = await _handler.Handle(new PropertyRequiredQuery
        {
            Responses = apiResponse,
            MockProperties = wiremockProps,
            Name = "UnitTest"
        }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Count.EqualTo(1));
            Assert.That(response[0].Type, Is.EqualTo(ValidatorType.ResponsePropertyRequired));
            Assert.That(response[0].Name, Is.EqualTo("Response - UnitTest - prop1"));
            Assert.That(response[0].ValidationResult, Is.EqualTo(ValidationResult.Warning));
            Assert.That(response[0].Description, Is.Not.Null.And.Contains("prop1"));
        });
    }


    [Test]
    public async Task Handle_MultipleMissingOptionalAndRequiredProperties()
    {
        var wiremockProps = new WiremockResponseProperties();
        wiremockProps.Properties.Add("Prop1", typeof(string));
        wiremockProps.Properties.Add("Prop4", typeof(string));

        OpenApiResponses apiResponse = RequiredOpenApiSchema();
        apiResponse["200"].Content["application/json"].Schema.Required.Add("Prop2");
        apiResponse["200"].Content["application/json"].Schema.Properties.Add("Prop2", new OpenApiSchema());
        apiResponse["200"].Content["application/json"].Schema.Properties.Add("Prop3", new OpenApiSchema());

        var response = await _handler.Handle(new PropertyRequiredQuery
        {
            Responses = apiResponse,
            MockProperties = wiremockProps,
            Name = "UnitTest"
        }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Count.EqualTo(3));
            Assert.That(response[0].Type, Is.EqualTo(ValidatorType.ResponsePropertyRequired));
            Assert.That(response[0].Name, Is.EqualTo("Response - UnitTest - prop1"));
            Assert.That(response[0].ValidationResult, Is.EqualTo(ValidationResult.Passed));
            Assert.That(response[0].Description, Is.Null.Or.Empty);
            Assert.That(response[1].Type, Is.EqualTo(ValidatorType.ResponsePropertyRequired));
            Assert.That(response[1].Name, Is.EqualTo("Response - UnitTest - Prop2"));
            Assert.That(response[1].ValidationResult, Is.EqualTo(ValidationResult.Failed));
            Assert.That(response[1].Description, Is.Not.Null.And.Contains("Prop2"));
            Assert.That(response[2].Type, Is.EqualTo(ValidatorType.ResponsePropertyRequired));
            Assert.That(response[2].Name, Is.EqualTo("Response - UnitTest - Prop3"));
            Assert.That(response[2].ValidationResult, Is.EqualTo(ValidationResult.Warning));
            Assert.That(response[2].Description, Is.Not.Null.And.Contains("Prop3"));
        });
    }

    private static OpenApiResponses RequiredOpenApiSchema()
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
                                {"prop1", new OpenApiSchema() }
                            }
                        }
                    } }
                }
            } }
        };
    }
}