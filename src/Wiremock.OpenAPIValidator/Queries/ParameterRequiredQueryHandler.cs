using Microsoft.OpenApi.Models;
using System.Text.Json;

namespace Wiremock.OpenAPIValidator.Queries;

public class ParameterRequiredQuery : BaseQuery
{
    public OpenApiParameter? Param { get; set; }
    public JsonElement MockedParameters { get; set; }
}

public class ParameterRequiredQueryHandler
{
    public Task<ValidatorNode> Handle(ParameterRequiredQuery request, CancellationToken cancellationToken)
    {
        if (request.Param == null)
        {
            return Task.FromResult(new ValidatorNode());
        }
        var existingProp = request.MockedParameters.TryGetProperty(request.Param.Name, out var _);

        if (!existingProp && request.Param.Required)
        {
            return Task.FromResult(new ValidatorNode
            {
                Name = request.Name,
                Description = $"Required Property '{request.Param.Name}' is not present in the mock",
                Type = ValidatorType.ParamRequired,
                ValidationResult = ValidationResult.Failed
            });
        }
        else if (!existingProp && !request.Param.Required)
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
}
