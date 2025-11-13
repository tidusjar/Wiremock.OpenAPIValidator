using System.Text.Json;
using System.Text.Json.Serialization;
using Wiremock.OpenAPIValidator.Models;

namespace Wiremock.OpenAPIValidator.Formatters;

public class JsonOutputFormatter : IOutputFormatter
{
    public string Format(ValidatorResults results, Options options)
    {
        var output = new JsonOutput
        {
            Timestamp = DateTime.UtcNow,
            Summary = new ValidationSummary
            {
                Total = results.Results.Count,
                Passed = results.Results.Count(x => x.ValidationResult == ValidationResult.Passed),
                Warning = results.Results.Count(x => x.ValidationResult == ValidationResult.Warning),
                Failed = results.Results.Count(x => x.ValidationResult == ValidationResult.Failed),
                Error = results.Results.Count(x => x.ValidationResult == ValidationResult.Error),
                IsValid = results.Valid
            },
            Results = results.Results.Select(r => new JsonValidationResult
            {
                Name = r.Name,
                Type = r.Type.ToString(),
                Result = r.ValidationResult.ToString(),
                Description = r.Description
            }).ToList()
        };

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };

        return JsonSerializer.Serialize(output, jsonOptions);
    }

    private class JsonOutput
    {
        public DateTime Timestamp { get; set; }
        public ValidationSummary Summary { get; set; } = new();
        public List<JsonValidationResult> Results { get; set; } = new();
    }

    private class ValidationSummary
    {
        public int Total { get; set; }
        public int Passed { get; set; }
        public int Warning { get; set; }
        public int Failed { get; set; }
        public int Error { get; set; }
        public bool IsValid { get; set; }
    }

    private class JsonValidationResult
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
