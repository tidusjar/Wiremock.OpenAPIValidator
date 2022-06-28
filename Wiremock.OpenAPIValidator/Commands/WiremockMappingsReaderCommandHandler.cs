using MediatR;
using System.Text.Json;

namespace Wiremock.OpenAPIValidator.Commands;

public class WiremockMappingsReaderCommand : IRequest<WiremockMappings>
{
    public string WiremockMappingPath { get; set; } = string.Empty;
}

internal class WiremockMappingsReaderCommandHandler : IRequestHandler<WiremockMappingsReaderCommand, WiremockMappings>
{
    public async Task<WiremockMappings> Handle(WiremockMappingsReaderCommand request, CancellationToken cancellationToken)
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
