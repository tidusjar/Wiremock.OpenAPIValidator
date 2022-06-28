using Microsoft.OpenApi.Models;
using Wiremock.OpenAPIValidator.Queries;

namespace Wiremock.OpenAPIValidator.Tests.Queries;

public class HttpMethodQueryHandlerTests
{

    private HttpMethodQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _handler = new HttpMethodQueryHandler();
    }

    [TestCase(OperationType.Put)]
    [TestCase(OperationType.Options)]
    [TestCase(OperationType.Delete)]
    [TestCase(OperationType.Patch)]
    [TestCase(OperationType.Head)]
    public async Task Handle_NotSupportedApiMethod(OperationType apiOperation)
    {
        var operations = new Dictionary<OperationType, OpenApiOperation>
        {
            { apiOperation, new OpenApiOperation { OperationId = "UnitTest"} },
            { OperationType.Post, new OpenApiOperation { OperationId = "UnitTest"} },
        };
        var response = await _handler.Handle(new HttpMethodQuery
        {
            RequestMethod = "GET",
            Api = new OpenApiPathItem
            {
                Operations = operations
            },
        }, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(response.Type, Is.EqualTo(ValidatorType.Method));
            Assert.That(response.Name, Is.EqualTo("UnitTest"));
            Assert.That(response.ValidationResult, Is.EqualTo(ValidationResult.Failed));
            Assert.That(response.Description, Is.Not.Null.And.Contains(apiOperation.ToString()));
        });
    }

    [TestCase("PUT")]
    [TestCase("OPTIONS")]
    [TestCase("DELETE")]
    [TestCase("PATCH")]
    [TestCase("POST")]
    [TestCase("HEAD")]
    [TestCase("TRACE")]
    public async Task Handle_NotSupportedApiMethod_RequestMethod(string operation)
    {
        var operations = new Dictionary<OperationType, OpenApiOperation>
        {
            { OperationType.Get, new OpenApiOperation { OperationId = "UnitTest"} },
        };
        var response = await _handler.Handle(new HttpMethodQuery
        {
            RequestMethod = operation,
            Api = new OpenApiPathItem
            {
                Operations = operations
            },
        }, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(response.Type, Is.EqualTo(ValidatorType.Method));
            Assert.That(response.Name, Is.EqualTo("UnitTest"));
            Assert.That(response.ValidationResult, Is.EqualTo(ValidationResult.Failed));
            Assert.That(response.Description, Is.Not.Null.And.Contains(OperationType.Get.ToString()));
        });
    }

    [Test]
    public void Handle_NotSupportedApiMethod_RequestMethodNotSupported()
    {
        var operations = new Dictionary<OperationType, OpenApiOperation>
        {
            { OperationType.Get, new OpenApiOperation { OperationId = "UnitTest"} },
        };
        Assert.ThrowsAsync<NotSupportedException>(async () => await _handler.Handle(new HttpMethodQuery
        {
            RequestMethod = "SOMEINVALIDONE",
            Api = new OpenApiPathItem
            {
                Operations = operations
            },
        }, CancellationToken.None));
    }

    [Test]
    public async Task Handle_SupportedApiMethod()
    {
        var operations = new Dictionary<OperationType, OpenApiOperation>
        {
            { OperationType.Get, new OpenApiOperation { OperationId = "UnitTest"} },
        };
        var response = await _handler.Handle(new HttpMethodQuery
        {
            RequestMethod = "GET",
            Api = new OpenApiPathItem
            {
                Operations = operations
            },
        }, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(response.Type, Is.EqualTo(ValidatorType.Method));
            Assert.That(response.Name, Is.EqualTo("UnitTest"));
            Assert.That(response.ValidationResult, Is.EqualTo(ValidationResult.Passed));
            Assert.That(response.Description, Is.Null.Or.Empty);
        });
    }
}