using Microsoft.OpenApi.Models;
using System.Text.Json;
using Wiremock.OpenAPIValidator.Queries;

namespace Wiremock.OpenAPIValidator.Tests.Queries;

public class UrlPathMatchQueryHandlerTests
{

    private UrlPathMatchQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _handler = new UrlPathMatchQueryHandler();
    }

    [Test]
    public async Task Handle_MissingApiPath()
    {
        var response = await _handler.Handle(new UrlPathMatchQuery
        {
            MockUrlPattern = "/api/v1/status/*",
            ApiPaths = new OpenApiPaths
            {
                { "/api/v1/something/info", new OpenApiPathItem() }
            }
        }, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(response.ValidationNode.Type, Is.EqualTo(ValidatorType.UrlMatch));
            Assert.That(response.ValidationNode.Name, Is.EqualTo("/api/v1/status/*"));
            Assert.That(response.ValidationNode.ValidationResult, Is.EqualTo(ValidationResult.Failed));
            Assert.That(response.ValidationNode.Description, Is.Not.Null.And.Contains("/api/v1/status/*"));
        });
    }

    [Test]
    public async Task Handle_MatchingApiPath()
    {
        var response = await _handler.Handle(new UrlPathMatchQuery
        {
            MockUrlPattern = @"/api/v1/status\\?.*",
            ApiPaths = new OpenApiPaths
            {
                { "/api/v1/status/info", new OpenApiPathItem() }
            }
        }, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(response.ValidationNode.Type, Is.EqualTo(ValidatorType.UrlMatch));
            Assert.That(response.ValidationNode.Name, Is.EqualTo(@"/api/v1/status\\?.*"));
            Assert.That(response.ValidationNode.ValidationResult, Is.EqualTo(ValidationResult.Passed));
        });
    }

    [Test]
    public async Task Handle_NullApiPath()
    {
        var response = await _handler.Handle(new UrlPathMatchQuery
        {
            MockUrlPattern = @"/api/v1/status\\?.*",
            ApiPaths = null
        }, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(response.ValidationNode, Is.InstanceOf<ValidatorNode>());
            Assert.That(response.MatchedPath, Is.Null);
        });
    }
}