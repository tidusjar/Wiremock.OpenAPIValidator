using MediatR;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Wiremock.OpenAPIValidator.Commands;

public class OpenApiDocumentReaderCommand : IRequest<OpenApiDocument>
{
    public string OpenApiDocLocation { get; set; } = string.Empty;
}

internal class OpenApiDocumentReaderHandler : IRequestHandler<OpenApiDocumentReaderCommand, OpenApiDocument>
{
    public async Task<OpenApiDocument> Handle(OpenApiDocumentReaderCommand request, CancellationToken cancellationToken)
    {
        if (!File.Exists(request.OpenApiDocLocation))
        {
            throw new FileNotFoundException($"The OpenAPI document path '{request.OpenApiDocLocation}' does not exist");
        }
        using var openApiStream = File.OpenRead(request.OpenApiDocLocation);
        var reader = new OpenApiStreamReader();
        var readResult = await reader.ReadAsync(openApiStream);

        return readResult.OpenApiDocument;
    }
}
