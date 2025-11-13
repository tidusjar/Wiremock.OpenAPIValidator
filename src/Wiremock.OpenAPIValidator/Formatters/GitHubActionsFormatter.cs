using System.Text;
using Wiremock.OpenAPIValidator.Models;

namespace Wiremock.OpenAPIValidator.Formatters;

public class GitHubActionsFormatter : IOutputFormatter
{
    public string Format(ValidatorResults results, Options options)
    {
        var output = new StringBuilder();

        // Summary as notice
        var summary = $"OpenAPI Validation: {results.Results.Count} checks - " +
                     $"{results.Results.Count(x => x.ValidationResult == ValidationResult.Passed)} passed, " +
                     $"{results.Results.Count(x => x.ValidationResult == ValidationResult.Warning)} warnings, " +
                     $"{results.Results.Count(x => x.ValidationResult == ValidationResult.Failed)} failed, " +
                     $"{results.Results.Count(x => x.ValidationResult == ValidationResult.Error)} errors";

        output.AppendLine($"::notice title=Validation Summary::{summary}");
        output.AppendLine();

        // Output each result as GitHub Actions annotation
        foreach (var result in results.Results)
        {
            var command = result.ValidationResult switch
            {
                ValidationResult.Failed => "error",
                ValidationResult.Error => "error",
                ValidationResult.Warning => "warning",
                ValidationResult.Passed => "notice",
                _ => "notice"
            };

            // Escape special characters in the message
            var message = EscapeGitHubActionsMessage(result.Description);
            var title = EscapeGitHubActionsMessage($"{result.Type}: {result.Name}");

            output.AppendLine($"::{command} title={title}::{message}");
        }

        // Add summary statistics
        output.AppendLine();
        output.AppendLine("## Validation Statistics");
        output.AppendLine($"- ✅ Passed: {results.Results.Count(x => x.ValidationResult == ValidationResult.Passed)}");
        output.AppendLine($"- ⚠️  Warning: {results.Results.Count(x => x.ValidationResult == ValidationResult.Warning)}");
        output.AppendLine($"- ❌ Failed: {results.Results.Count(x => x.ValidationResult == ValidationResult.Failed)}");
        output.AppendLine($"- 🔴 Error: {results.Results.Count(x => x.ValidationResult == ValidationResult.Error)}");

        return output.ToString();
    }

    private static string EscapeGitHubActionsMessage(string message)
    {
        // Escape special characters according to GitHub Actions workflow command syntax
        return message
            .Replace("%", "%25")
            .Replace("\r", "%0D")
            .Replace("\n", "%0A")
            .Replace(":", "%3A")
            .Replace(",", "%2C");
    }
}
