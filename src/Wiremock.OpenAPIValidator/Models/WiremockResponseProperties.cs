namespace Wiremock.OpenAPIValidator.Models;

public class WiremockResponseProperties
{
    public Dictionary<string, Type> Properties { get; set; } = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
    public ObjectType ObjectType { get; set; }
}

public enum ObjectType
{
    Object,
    Array
}