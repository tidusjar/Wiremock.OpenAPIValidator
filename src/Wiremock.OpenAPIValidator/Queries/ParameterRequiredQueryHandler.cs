using Microsoft.OpenApi.Models;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Wiremock.OpenAPIValidator.Queries;

public class ParameterRequiredQuery : BaseQuery
{
    public OpenApiParameter? Param { get; set; }
    public string? MockedParameters { get; set; }
}

public class ParameterRequiredQueryHandler
{
    public Task<ValidatorNode> Handle(ParameterRequiredQuery request, CancellationToken cancellationToken)
    {
        if (request.Param == null)
        {
            return Task.FromResult(new ValidatorNode());
        }

        var exists = ParameterExists(request.Param.Name, request.MockedParameters);

        if (!exists && request.Param.Required)
        {
            return Task.FromResult(new ValidatorNode
            {
                Name = request.Name,
                Description = $"Required Property '{request.Param.Name}' is not present in the mock",
                Type = ValidatorType.ParamRequired,
                ValidationResult = ValidationResult.Failed
            });
        }
        else if (!exists && !request.Param.Required)
        {
            return Task.FromResult(new ValidatorNode
            {
                Name = request.Name,
                Description = $"Optional Property '{request.Param.Name}' is not present in the mock",
                Type = ValidatorType.ParamRequired,
                ValidationResult = ValidationResult.Warning
            });
        }

        return Task.FromResult(new ValidatorNode
        {
            Name = request.Name,
            Type = ValidatorType.ParamRequired,
            ValidationResult = ValidationResult.Passed
        });
    }

    private static bool ParameterExists(string paramName, string? mockedParameter)
    {
        if (mockedParameter is null)
        {
            return false;
        }

        return JsonNode.Parse(mockedParameter) is JsonObject json && json.ContainsKey(paramName);
    }
}
