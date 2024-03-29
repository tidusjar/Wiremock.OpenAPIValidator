﻿using System.Text.Json.Serialization;

namespace Wiremock.OpenAPIValidator
{
    public class WiremockRequest
    {
        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;

        [JsonPropertyName("urlPattern")]
        public string _urlPattern { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonIgnore]
        public string UrlPattern
        {
            get { return _urlPattern.Replace(@"\", @"\\"); }
            set { _urlPattern = @value; }
        }

        [JsonPropertyName("queryParameters")]
        public dynamic? QueryParameters { get; set; }
    }
}