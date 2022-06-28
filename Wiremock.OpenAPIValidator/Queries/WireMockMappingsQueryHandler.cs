using MediatR;

namespace Wiremock.OpenAPIValidator.Queries;

public class WireMockMappingsQuery : IRequest<string[]>
{
    public string MappingsPath { get; set; } = string.Empty;
}
internal class WireMockMappingsQueryHandler : IRequestHandler<WireMockMappingsQuery, string[]>
{
    public Task<string[]> Handle(WireMockMappingsQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(Directory.GetFiles(request.MappingsPath));

}