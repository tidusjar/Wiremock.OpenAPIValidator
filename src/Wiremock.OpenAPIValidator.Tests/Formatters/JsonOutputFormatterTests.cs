using System.Text.Json;
using Wiremock.OpenAPIValidator.Formatters;
using Wiremock.OpenAPIValidator.Models;

namespace Wiremock.OpenAPIValidator.Tests.Formatters;

public class JsonOutputFormatterTests
{
    private JsonOutputFormatter _formatter;
    private Options _options;

    [SetUp]
    public void Setup()
    {
        _formatter = new JsonOutputFormatter();
        _options = new Options
        {
            OpenApiPath = "test.yml",
            WiremockMappingsPath = "mappings/",
            Format = "json"
        };
    }

    [Test]
    public void Format_ValidResults_ReturnsValidJson()
    {
        // Arrange
        var results = new ValidatorResults
        {
            Results = new List<ValidatorNode>
            {
                new ValidatorNode
                {
                    Name = "Test1",
                    Type = ValidatorType.Method,
                    ValidationResult = ValidationResult.Passed,
                    Description = "Test passed"
                },
                new ValidatorNode
                {
                    Name = "Test2",
                    Type = ValidatorType.Response,
                    ValidationResult = ValidationResult.Failed,
                    Description = "Test failed"
                }
            }
        };

        // Act
        var output = _formatter.Format(results, _options);

        // Assert
        Assert.That(output, Is.Not.Null.And.Not.Empty);

        // Verify it's valid JSON
        var jsonDoc = JsonDocument.Parse(output);
        Assert.That(jsonDoc, Is.Not.Null);
    }

    [Test]
    public void Format_IncludesSummary()
    {
        // Arrange
        var results = new ValidatorResults
        {
            Results = new List<ValidatorNode>
            {
                new ValidatorNode { Name = "T1", Type = ValidatorType.Method, ValidationResult = ValidationResult.Passed, Description = "P" },
                new ValidatorNode { Name = "T2", Type = ValidatorType.Response, ValidationResult = ValidationResult.Failed, Description = "F" },
                new ValidatorNode { Name = "T3", Type = ValidatorType.ParamRequired, ValidationResult = ValidationResult.Warning, Description = "W" },
                new ValidatorNode { Name = "T4", Type = ValidatorType.ParamType, ValidationResult = ValidationResult.Error, Description = "E" }
            }
        };

        // Act
        var output = _formatter.Format(results, _options);
        var jsonDoc = JsonDocument.Parse(output);

        // Assert
        var summary = jsonDoc.RootElement.GetProperty("summary");
        Assert.Multiple(() =>
        {
            Assert.That(summary.GetProperty("total").GetInt32(), Is.EqualTo(4));
            Assert.That(summary.GetProperty("passed").GetInt32(), Is.EqualTo(1));
            Assert.That(summary.GetProperty("failed").GetInt32(), Is.EqualTo(1));
            Assert.That(summary.GetProperty("warning").GetInt32(), Is.EqualTo(1));
            Assert.That(summary.GetProperty("error").GetInt32(), Is.EqualTo(1));
            Assert.That(summary.GetProperty("isValid").GetBoolean(), Is.False);
        });
    }

    [Test]
    public void Format_IncludesTimestamp()
    {
        // Arrange
        var results = new ValidatorResults { Results = new List<ValidatorNode>() };

        // Act
        var output = _formatter.Format(results, _options);
        var jsonDoc = JsonDocument.Parse(output);

        // Assert
        var timestamp = jsonDoc.RootElement.GetProperty("timestamp").GetDateTime();
        Assert.That(timestamp, Is.Not.EqualTo(default(DateTime)));
        Assert.That(timestamp, Is.LessThanOrEqualTo(DateTime.UtcNow.AddSeconds(1)));
    }

    [Test]
    public void Format_IncludesAllResults()
    {
        // Arrange
        var results = new ValidatorResults
        {
            Results = new List<ValidatorNode>
            {
                new ValidatorNode { Name = "Test1", Type = ValidatorType.Method, ValidationResult = ValidationResult.Passed, Description = "Desc1" },
                new ValidatorNode { Name = "Test2", Type = ValidatorType.Response, ValidationResult = ValidationResult.Failed, Description = "Desc2" }
            }
        };

        // Act
        var output = _formatter.Format(results, _options);
        var jsonDoc = JsonDocument.Parse(output);

        // Assert
        var resultsArray = jsonDoc.RootElement.GetProperty("results");
        Assert.That(resultsArray.GetArrayLength(), Is.EqualTo(2));

        var firstResult = resultsArray[0];
        Assert.Multiple(() =>
        {
            Assert.That(firstResult.GetProperty("name").GetString(), Is.EqualTo("Test1"));
            Assert.That(firstResult.GetProperty("type").GetString(), Is.EqualTo("Method"));
            Assert.That(firstResult.GetProperty("result").GetString(), Is.EqualTo("Passed"));
            Assert.That(firstResult.GetProperty("description").GetString(), Is.EqualTo("Desc1"));
        });
    }
}
