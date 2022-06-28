using System.Text.Json.Serialization;

namespace Wiremock.OpenAPIValidator
{
    public class WiremockHeaders
    {
        [JsonPropertyName("Content-Type")]
        public string ContentType { get; set; }
    }
}