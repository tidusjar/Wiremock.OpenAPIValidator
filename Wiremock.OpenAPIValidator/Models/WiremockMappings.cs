using System.Text.Json.Serialization;

namespace Wiremock.OpenAPIValidator
{
    public class WiremockMappings
    {
        [JsonPropertyName("request")]
        public WiremockRequest Request { get; set; }
        [JsonPropertyName("response")]
        public WiremockResponse Response { get; set; }

    }
}