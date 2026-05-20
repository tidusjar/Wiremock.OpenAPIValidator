using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Wiremock.OpenAPIValidator.Queries;

public class ParameterTypeQuery : BaseQuery
{
    public OpenApiParameter? Param { get; set; }
    public string? MockedParameters { get; set; }
}

public class ParameterTypeQueryHandler
{
    public Task<ValidatorNode> Handle(ParameterTypeQuery request, CancellationToken cancellationToken)
    {
        if (request.Param == null)
        {
            return Task.FromResult(new ValidatorNode());
        }

        var propExists = TryParseAsObject(request.MockedParameters, out var mockedProperty);
        if (!propExists && request.Param.Required)
        {
            return Task.FromResult(new ValidatorNode
            {
                Name = request.Name,
                Description = $"Required Property '{request.Param.Name}' is not present in the mock",
                Type = ValidatorType.ParamType,
                ValidationResult = ValidationResult.Failed
            });
        }
        else if (!propExists && !request.Param.Required)
        {
            return Task.FromResult(new ValidatorNode
            {
                Name = request.Name,
                Type = ValidatorType.ParamType,
                Description = $"Parameter {request.Param.Name} is not required and doesn't exist in the mock. Cannot check the Type mappings.",
                ValidationResult = ValidationResult.Warning
            });
        }

        // Check if the type is correct
        var result = mockedProperty.Deserialize<Dictionary<string, string>>();
        if (result == null)
        {
            return Task.FromResult(new ValidatorNode
            {
                Name = request.Name,
                Type = ValidatorType.ParamType,
                Description = $"Parameter {request.Param.Name} couldn't be deseralized and we cannot check",
                ValidationResult = ValidationResult.Error
            });
        }

        if (request.Param.Schema.Enum.Any())
        {
            // enum check
            var apiEnums = request.Param.Schema.Enum.Any(x => result.ContainsValue(((OpenApiString)x).Value));
            if (!apiEnums)
            {
                return Task.FromResult(new ValidatorNode
                {
                    Name = request.Name,
                    Description = $"Required Property '{request.Param.Name}' is an enum, the mocked value '{string.Join(',', result.Values.ToArray())}' is not a valid value for that enum",
                    Type = ValidatorType.ParamType,
                    ValidationResult = ValidationResult.Failed
                });
            }
            return Task.FromResult(new ValidatorNode
            {
                Name = request.Name,
                Type = ValidatorType.ParamType,
                ValidationResult = ValidationResult.Passed
            });
        }

        var apiType = GetSchemeProprtyType(request.Param.Schema.Format);
        if (result.Keys.First().Equals("equalTo"))
        {
            var val = result.Values.First();
            var castType = TryConvertTo(val, apiType);
            if (castType == null)
            {
                return Task.FromResult(new ValidatorNode
                {
                    Name = request.Name,
                    Description = $"Required Property'{request.Param.Name}' is not the correct type. API Expects: '{apiType.FullName}', Mocked Value: '{val}'",
                    Type = ValidatorType.ParamType,
                    ValidationResult = ValidationResult.Failed
                });
            }
        }

        if (result.TryGetValue("matches", out var regexMatch))
        {
            // Check if the regex matches the type
            var regex = new Regex(regexMatch);
            if (apiType == typeof(Guid))
            {
                if (!regex.IsMatch(Guid.NewGuid().ToString()))
                {
                    return Task.FromResult(new ValidatorNode
                    {
                        Name = request.Name,
                        Description = $"Required Property '{request.Param.Name}' regex does not match the correct type. API Provides: '{Guid.NewGuid()}', Mocked Match: '{regexMatch}'",
                        Type = ValidatorType.ParamType,
                        ValidationResult = ValidationResult.Failed
                    });
                }
            } 
            else
            {
                return Task.FromResult(new ValidatorNode
                {
                    Name = request.Name,
                    Description = $"Required Property '{request.Param.Name}' Match is not yet supported, Mocked Match: '{regexMatch}'",
                    Type = ValidatorType.ParamType,
                    ValidationResult = ValidationResult.Warning
                });
            }
        }

        return Task.FromResult(new ValidatorNode
        {
            Name = request.Name,
            Type = ValidatorType.ParamType,
            ValidationResult = ValidationResult.Passed
        });
    }

    public static object? TryConvertTo(string input, Type type)
    {
        object? result = null;
        try
        {
            result = Convert.ChangeType(input, type);
        }
        catch
        {
            // Ignore because we will just return null on invalid cast
        }

        return result;
    }

    private static Type GetSchemeProprtyType(string format) => format switch
    {
        "date-time" => typeof(DateTime),
        "uuid" => typeof(Guid),
        "string" => typeof(string),
        "int32" => typeof(int),
        "int64" => typeof(long),
        _ => throw new NotImplementedException(),
    };

    private static bool TryParseAsObject(string? input, [NotNullWhen(true)] out JsonObject? json)
    {
        if (input is not null && JsonNode.Parse(input) is JsonObject obj)
        {
            json = obj;
            return true;
        }

        json = null;
        return false;
    }
}
