namespace Wiremock.OpenAPIValidator
{
    public class ValidatorNode
    {
        public string Name { get; set; } = string.Empty;
        public ValidatorType Type { get; set; }
        public ValidationResult ValidationResult { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public enum ValidationResult
    {
        Passed,
        Warning,
        Failed,
        Error
    }
}