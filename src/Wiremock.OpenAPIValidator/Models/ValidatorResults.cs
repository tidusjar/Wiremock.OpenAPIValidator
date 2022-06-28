namespace Wiremock.OpenAPIValidator
{
    public class ValidatorResults
    {
        public List<ValidatorNode> Results { get; set; } = new List<ValidatorNode>();
        public bool Valid => Results.All(x => x.ValidationResult == ValidationResult.Passed);
    }
}