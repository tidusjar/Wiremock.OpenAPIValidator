using MediatR;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using Wiremock.OpenAPIValidator.Models;

namespace Wiremock.OpenAPIValidator.Queries;

public class PropertyRequiredQuery : BaseQuery, IRequest<List<ValidatorNode>>
{
    public OpenApiResponses Responses { get; set; }
    public WiremockResponseProperties MockProperties { get; set; }
}

public class PropertyRequiredQueryHandler : IRequestHandler<PropertyRequiredQuery, List<ValidatorNode>>
{
    public Task<List<ValidatorNode>> Handle(PropertyRequiredQuery request, CancellationToken cancellationToken)
    {
        var response = new List<ValidatorNode>();
        // Only check for 200 OK response
        var okResponse = request.Responses.First(x => x.Key == "200");
        // Get JSON body
        var jsonBody = okResponse.Value.Content.First(x => x.Key == "application/json");
        var schema = jsonBody.Value.Schema;

        foreach (var property in schema.Properties)
        {
            var mockedProperty = request.MockProperties.Properties.TryGetValue(property.Key, out var mockedValue);
            var required = schema.Required.Any(x => x == property.Key);
            if (!mockedProperty && required)
            {
                response.Add(new ValidatorNode
                {
                    Name = GetName(request.Name, property.Key),
                    Description = $"Required Response Property '{property.Key}' is not present in the mocked response",
                    Type = ValidatorType.ResponsePropertyRequired,
                    ValidationResult = ValidationResult.Failed
                });
            }
            else if (!mockedProperty && !required)
            {
                response.Add(new ValidatorNode
                {
                    Name = GetName(request.Name, property.Key),
                    Description = $"Optional Response Property '{property.Key}' is not present in the mocked response",
                    Type = ValidatorType.ResponsePropertyRequired,
                    ValidationResult = ValidationResult.Warning
                });
            }

            response.Add(new ValidatorNode
            {
                Name = GetName(request.Name, property.Key),
                Type = ValidatorType.ResponsePropertyRequired,
                ValidationResult = ValidationResult.Passed
            });

        }
        return Task.FromResult(response);
    }

    private static string GetName(string name, string property) => $"{name} - {property}";
}