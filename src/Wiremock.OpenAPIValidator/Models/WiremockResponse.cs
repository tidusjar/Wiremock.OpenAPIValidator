using System.Net;
using System.Text.Json.Serialization;

namespace Wiremock.OpenAPIValidator
{
    public class WiremockResponse
    {
        [JsonPropertyName("status")]
        public HttpStatusCode Status { get; set; }
        [JsonPropertyName("bodyFileName")]
        public string FileName { get; set; } = string.Empty;
        [JsonPropertyName("headers")]
        public WiremockHeaders? Headers { get; set; }
    }
}