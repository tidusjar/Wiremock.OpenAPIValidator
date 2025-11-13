using System.Text.Json;

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
        var mappings = await JsonSerializer.DeserializeAsync<WiremockMappings>(stream, cancellationToken: cancellationToken);


        return mappings;
    }
}
