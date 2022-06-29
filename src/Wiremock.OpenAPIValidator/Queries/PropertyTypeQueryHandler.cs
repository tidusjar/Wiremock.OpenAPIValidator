using MediatR;
using Microsoft.OpenApi.Models;
using Wiremock.OpenAPIValidator.Models;

namespace Wiremock.OpenAPIValidator.Queries;

public class PropertyTypeQuery : BaseQuery, IRequest<List<ValidatorNode>>
{
    public OpenApiResponses? Responses { get; set; }
    public WiremockResponseProperties? MockProperties { get; set; }
}

public class PropertyTypeQueryHandler : IRequestHandler<PropertyTypeQuery, List<ValidatorNode>>
{
    public Task<List<ValidatorNode>> Handle(PropertyTypeQuery request, CancellationToken cancellationToken)
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

        foreach (var property in schema.Properties)
        {
            var mockedProperty = request.MockProperties.Properties.TryGetValue(property.Key, out var mockedValue);

            if (!mockedProperty)
            {
                response.Add(new ValidatorNode
                {
                    Name = GetName(request.Name, property.Key),
                    Description = $"Property '{property.Key}' is not present in the mock",
                    Type = ValidatorType.ResponsePropertyType,
                    ValidationResult = ValidationResult.Warning
                });
                continue;
            }

            // Check the type
            var apiType = GetSchemeProprtyType(property.Value.Type);
            if (apiType != mockedValue)
            {
                response.Add(new ValidatorNode
                {
                    Name = GetName(request.Name, property.Key),
                    Description = $"Required Property '{property.Key}' is not present in the mock",
                    Type = ValidatorType.ResponsePropertyType,
                    ValidationResult = ValidationResult.Failed
                });
            } 
            else
            {
                response.Add(new ValidatorNode
                {
                    Name = GetName(request.Name, property.Key),
                    Type = ValidatorType.ResponsePropertyType,
                    ValidationResult = ValidationResult.Passed
                });
            }
        }
        return Task.FromResult(response);
    }

    private static string GetName(string name, string property) => QueryHelpers.GetName(false, name, property);

    private static Type GetSchemeProprtyType(string format) => format switch
    {
        "date-time" => typeof(DateTime),
        "uuid" => typeof(Guid),
        "string" => typeof(string),
        "integer" => typeof(int),
        "number" => typeof(int),
        _ => throw new NotImplementedException(),
    };
}