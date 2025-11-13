namespace Wiremock.OpenAPIValidator.Queries;

public class WireMockMappingsQuery
{
    public string MappingsPath { get; set; } = string.Empty;
}
public class WireMockMappingsQueryHandler
{
    public Task<string[]> Handle(WireMockMappingsQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(Directory.GetFiles(request.MappingsPath));

}