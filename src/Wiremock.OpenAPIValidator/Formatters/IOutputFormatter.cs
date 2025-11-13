using Wiremock.OpenAPIValidator.Models;

namespace Wiremock.OpenAPIValidator.Formatters;

public interface IOutputFormatter
{
    /// <summary>
    /// Formats the validation results and returns the output as a string
    /// </summary>
    /// <param name="results">The validation results to format</param>
    /// <param name="options">CLI options that may affect formatting</param>
    /// <returns>Formatted output string</returns>
    string Format(ValidatorResults results, Options options);
}
