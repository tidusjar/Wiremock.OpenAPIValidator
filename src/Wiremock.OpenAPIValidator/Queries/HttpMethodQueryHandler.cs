using Microsoft.OpenApi.Models;

namespace Wiremock.OpenAPIValidator.Queries;

public class HttpMethodQuery : BaseQuery
{
    public OpenApiPathItem Api { get; set; } = new OpenApiPathItem();
    public string RequestMethod { get; set; } = string.Empty;
}

public class HttpMethodQueryHandler
{
    public Task<ValidatorNode> Handle(HttpMethodQuery request, CancellationToken cancellationToken)
    {
        var supportedApiMethods = request.Api.Operations.Select(x => x.Key);
        var mockMethod = new List<OperationType> { MethodToOperationType(request.RequestMethod) };

        var supportedMethods = supportedApiMethods.All(mockMethod.Contains);
        if (!supportedMethods)
        {
            return Task.FromResult(new ValidatorNode
            {
                Name = request.Api.Operations.Select(x => x.Value.OperationId).First(),
                Description = $"Method '{string.Join(',', supportedApiMethods.Except(mockMethod))}' is not covered by any mocks",
                Type = ValidatorType.Method,
                ValidationResult = ValidationResult.Failed
            });
        }
        return Task.FromResult(new ValidatorNode
        {
            Name = request.Api.Operations.Select(x => x.Value.OperationId).First(),
            Type = ValidatorType.Method,
            ValidationResult = ValidationResult.Passed
        });
    }

    private static OperationType MethodToOperationType(string method) => method switch
    {
        "GET" => OperationType.Get,
        "POST" => OperationType.Post,
        "PUT" => OperationType.Put,
        "DELETE" => OperationType.Delete,
        "OPTIONS" => OperationType.Options,
        "PATCH" => OperationType.Patch,
        "HEAD" => OperationType.Head,
        "TRACE" => OperationType.Trace,
        _ => throw new NotSupportedException(),
    };
}
