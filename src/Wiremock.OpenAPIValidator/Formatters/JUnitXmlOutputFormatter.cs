using System.Text;
using System.Xml;
using Wiremock.OpenAPIValidator.Models;

namespace Wiremock.OpenAPIValidator.Formatters;

public class JUnitXmlOutputFormatter : IOutputFormatter
{
    public string Format(ValidatorResults results, Options options)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = false
        };

        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, settings);

        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("testsuites");

        // Group by validator type
        var groupedResults = results.Results.GroupBy(r => r.Type);

        foreach (var group in groupedResults)
        {
            var testCases = group.ToList();
            var failures = testCases.Count(x => x.ValidationResult == ValidationResult.Failed);
            var errors = testCases.Count(x => x.ValidationResult == ValidationResult.Error);
            var skipped = testCases.Count(x => x.ValidationResult == ValidationResult.Warning);

            xmlWriter.WriteStartElement("testsuite");
            xmlWriter.WriteAttributeString("name", $"WireMock.OpenAPI.{group.Key}");
            xmlWriter.WriteAttributeString("tests", testCases.Count.ToString());
            xmlWriter.WriteAttributeString("failures", failures.ToString());
            xmlWriter.WriteAttributeString("errors", errors.ToString());
            xmlWriter.WriteAttributeString("skipped", skipped.ToString());
            xmlWriter.WriteAttributeString("timestamp", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"));

            foreach (var result in testCases)
            {
                xmlWriter.WriteStartElement("testcase");
                xmlWriter.WriteAttributeString("name", result.Name);
                xmlWriter.WriteAttributeString("classname", $"WireMock.OpenAPI.{result.Type}");

                switch (result.ValidationResult)
                {
                    case ValidationResult.Failed:
                        xmlWriter.WriteStartElement("failure");
                        xmlWriter.WriteAttributeString("message", result.Description);
                        xmlWriter.WriteAttributeString("type", "ValidationFailure");
                        xmlWriter.WriteString(result.Description);
                        xmlWriter.WriteEndElement(); // failure
                        break;

                    case ValidationResult.Error:
                        xmlWriter.WriteStartElement("error");
                        xmlWriter.WriteAttributeString("message", result.Description);
                        xmlWriter.WriteAttributeString("type", "ValidationError");
                        xmlWriter.WriteString(result.Description);
                        xmlWriter.WriteEndElement(); // error
                        break;

                    case ValidationResult.Warning:
                        xmlWriter.WriteStartElement("skipped");
                        xmlWriter.WriteAttributeString("message", result.Description);
                        xmlWriter.WriteEndElement(); // skipped
                        break;

                    case ValidationResult.Passed:
                        // No additional element needed for passed tests
                        break;
                }

                xmlWriter.WriteEndElement(); // testcase
            }

            xmlWriter.WriteEndElement(); // testsuite
        }

        xmlWriter.WriteEndElement(); // testsuites
        xmlWriter.WriteEndDocument();
        xmlWriter.Flush();

        return stringWriter.ToString();
    }
}
