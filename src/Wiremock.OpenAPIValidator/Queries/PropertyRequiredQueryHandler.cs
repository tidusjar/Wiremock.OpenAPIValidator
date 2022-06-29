using MediatR;
using Microsoft.OpenApi.Models;
using Wiremock.OpenAPIValidator.Models;

namespace Wiremock.OpenAPIValidator.Queries;

public class PropertyRequiredQuery : BaseQuery, IRequest<List<ValidatorNode>>
{
    public OpenApiResponses? Responses { get; set; }
    public WiremockResponseProperties? MockProperties { get; set; }
}

public class PropertyRequiredQueryHandler : IRequestHandler<PropertyRequiredQuery, List<ValidatorNode>>
{
    public Task<List<ValidatorNode>> Handle(PropertyRequiredQuery request, CancellationToken cancellationToken)
    {
        var response = new List<ValidatorNode>();
        if (request.Responses == null || request.MockProperties == null)
        {
            return Task.FromResult(response);
        }
        // Only check for 200 OK response
        var okResponse = request.Responses.First(x => x.Key == "200");
        // Get JSON body
        var jsonBody = okResponse.Value.Content.First(x => x.Key == "application/json");
        var schema = jsonBody.Value.Schema;

        foreach (var property in schema.Properties.Select(p => p.Key))
        {
            var mockedProperty = request.MockProperties.Properties.TryGetValue(property, out var mockedValue);
            var required = schema.Required.Any(x => x == property);
            if (!mockedProperty && required)
            {
                response.Add(new ValidatorNode
                {
                    Name = GetName(request.Name, property),
                    Description = $"Required Response Property '{property}' is not present in the mocked response",
                    Type = ValidatorType.ResponsePropertyRequired,
                    ValidationResult = ValidationResult.Failed
                });
            }
            else if (!mockedProperty)
            {
                response.Add(new ValidatorNode
                {
                    Name = GetName(request.Name, property),
                    Description = $"Optional Response Property '{property}' is not present in the mocked response",
                    Type = ValidatorType.ResponsePropertyRequired,
                    ValidationResult = ValidationResult.Warning
                });
            }
            else
            {
                response.Add(new ValidatorNode
                {
                    Name = GetName(request.Name, property),
                    Type = ValidatorType.ResponsePropertyRequired,
                    ValidationResult = ValidationResult.Passed
                });
            }
        }
        return Task.FromResult(response);
    }

    private static string GetName(string name, string property) => QueryHelpers.GetName(false, name, property);
}