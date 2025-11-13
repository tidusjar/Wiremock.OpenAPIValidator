using System.Xml;
using Wiremock.OpenAPIValidator.Formatters;
using Wiremock.OpenAPIValidator.Models;

namespace Wiremock.OpenAPIValidator.Tests.Formatters;

public class JUnitXmlOutputFormatterTests
{
    private JUnitXmlOutputFormatter _formatter;
    private Options _options;

    [SetUp]
    public void Setup()
    {
        _formatter = new JUnitXmlOutputFormatter();
        _options = new Options
        {
            OpenApiPath = "test.yml",
            WiremockMappingsPath = "mappings/",
            Format = "junit"
        };
    }

    [Test]
    public void Format_ValidResults_ReturnsValidXml()
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
                }
            }
        };

        // Act
        var output = _formatter.Format(results, _options);

        // Assert
        Assert.That(output, Is.Not.Null.And.Not.Empty);

        // Verify it's valid XML
        var xmlDoc = new XmlDocument();
        Assert.DoesNotThrow(() => xmlDoc.LoadXml(output));
    }

    [Test]
    public void Format_GroupsByValidatorType()
    {
        // Arrange
        var results = new ValidatorResults
        {
            Results = new List<ValidatorNode>
            {
                new ValidatorNode { Name = "T1", Type = ValidatorType.Method, ValidationResult = ValidationResult.Passed, Description = "P1" },
                new ValidatorNode { Name = "T2", Type = ValidatorType.Method, ValidationResult = ValidationResult.Failed, Description = "F1" },
                new ValidatorNode { Name = "T3", Type = ValidatorType.Response, ValidationResult = ValidationResult.Passed, Description = "P2" }
            }
        };

        // Act
        var output = _formatter.Format(results, _options);
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(output);

        // Assert - should have 2 test suites (Method and Response)
        var testSuites = xmlDoc.SelectNodes("//testsuite");
        Assert.That(testSuites?.Count, Is.EqualTo(2));
    }

    [Test]
    public void Format_FailuresMarkedCorrectly()
    {
        // Arrange
        var results = new ValidatorResults
        {
            Results = new List<ValidatorNode>
            {
                new ValidatorNode
                {
                    Name = "FailedTest",
                    Type = ValidatorType.Method,
                    ValidationResult = ValidationResult.Failed,
                    Description = "This test failed"
                }
            }
        };

        // Act
        var output = _formatter.Format(results, _options);
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(output);

        // Assert
        var testSuite = xmlDoc.SelectSingleNode("//testsuite");
        Assert.Multiple(() =>
        {
            Assert.That(testSuite?.Attributes?["failures"]?.Value, Is.EqualTo("1"));
            Assert.That(testSuite?.Attributes?["errors"]?.Value, Is.EqualTo("0"));
        });

        var failureNode = xmlDoc.SelectSingleNode("//failure");
        Assert.That(failureNode, Is.Not.Null);
        Assert.That(failureNode?.Attributes?["message"]?.Value, Is.EqualTo("This test failed"));
    }

    [Test]
    public void Format_ErrorsMarkedCorrectly()
    {
        // Arrange
        var results = new ValidatorResults
        {
            Results = new List<ValidatorNode>
            {
                new ValidatorNode
                {
                    Name = "ErrorTest",
                    Type = ValidatorType.Response,
                    ValidationResult = ValidationResult.Error,
                    Description = "This is an error"
                }
            }
        };

        // Act
        var output = _formatter.Format(results, _options);
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(output);

        // Assert
        var testSuite = xmlDoc.SelectSingleNode("//testsuite");
        Assert.Multiple(() =>
        {
            Assert.That(testSuite?.Attributes?["errors"]?.Value, Is.EqualTo("1"));
            Assert.That(testSuite?.Attributes?["failures"]?.Value, Is.EqualTo("0"));
        });

        var errorNode = xmlDoc.SelectSingleNode("//error");
        Assert.That(errorNode, Is.Not.Null);
        Assert.That(errorNode?.Attributes?["message"]?.Value, Is.EqualTo("This is an error"));
    }

    [Test]
    public void Format_WarningsMarkedAsSkipped()
    {
        // Arrange
        var results = new ValidatorResults
        {
            Results = new List<ValidatorNode>
            {
                new ValidatorNode
                {
                    Name = "WarningTest",
                    Type = ValidatorType.ParamRequired,
                    ValidationResult = ValidationResult.Warning,
                    Description = "This is a warning"
                }
            }
        };

        // Act
        var output = _formatter.Format(results, _options);
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(output);

        // Assert
        var testSuite = xmlDoc.SelectSingleNode("//testsuite");
        Assert.That(testSuite?.Attributes?["skipped"]?.Value, Is.EqualTo("1"));

        var skippedNode = xmlDoc.SelectSingleNode("//skipped");
        Assert.That(skippedNode, Is.Not.Null);
    }
}
