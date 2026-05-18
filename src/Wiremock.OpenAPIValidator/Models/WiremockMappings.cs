using System.Text.Json.Serialization;

namespace Wiremock.OpenAPIValidator
{
    public class WiremockMappings
    {
        [JsonPropertyName("mappings")]
        public List<WiremockMapping> Mappings { get; set; } = new List<WiremockMapping>();
    }

    public class WiremockMapping
    {
        [JsonPropertyName("request")]
        public WiremockRequest? Request { get; set; }
        [JsonPropertyName("response")]
        public WiremockResponse? Response { get; set; }

    }
}