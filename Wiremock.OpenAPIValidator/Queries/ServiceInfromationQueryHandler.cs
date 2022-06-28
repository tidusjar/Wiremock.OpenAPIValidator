using MediatR;

namespace Wiremock.OpenAPIValidator.Queries;

public class ServiceInformationQuery : IRequest<string>
{
}
internal class ServiceInfromationQueryHandler : IRequestHandler<ServiceInformationQuery, string>
{
    public Task<string> Handle(ServiceInformationQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
