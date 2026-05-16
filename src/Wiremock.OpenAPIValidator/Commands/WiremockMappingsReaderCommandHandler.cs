using System.Text.Json;
using System.Text.Json.Nodes;

namespace Wiremock.OpenAPIValidator.Commands;

public class WiremockMappingsReaderCommand
{
    public string WiremockMappingPath { get; set; } = string.Empty;
}

public class WiremockMappingsReaderCommandHandler
{
    public async Task<WiremockMappings?> Handle(WiremockMappingsReaderCommand request, CancellationToken cancellationToken)
    {
        if (!File.Exists(request.WiremockMappingPath))
        {
            throw new FileNotFoundException($"The Wiremock Mappings path '{request.WiremockMappingPath}' does not exist");
        }

        using var stream = File.OpenRead(request.WiremockMappingPath);
        var json = (await JsonNode.ParseAsync(stream))!.AsObject();

        var mappings = new WiremockMappings();

        if (json.FirstOrDefault().Value is JsonArray arr && (arr.Count == 0 || arr[0]!["request"] != null))
        {
            mappings = JsonSerializer.Deserialize<WiremockMappings>(json);
            return mappings;
        }
        if (json.FirstOrDefault().Value is JsonObject)
        {
            var mapping = JsonSerializer.Deserialize<WiremockMapping?>(json);
            mappings?.Mappings.Add(mapping!);
            return mappings;
        }

        return null;
    }
}
