namespace Wiremock.OpenAPIValidator.Queries
{
    public static class QueryHelpers
    {
        public static string GetName(bool request, string name, string property) => $"{(request ? "Request" : "Response")} - {name} - {property}";
    }
}
