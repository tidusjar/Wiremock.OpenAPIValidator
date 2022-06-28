using MediatR;
using Microsoft.OpenApi.Models;
using System.Text.RegularExpressions;

namespace Wiremock.OpenAPIValidator.Queries;

public class UrlPathMatchQuery : IRequest<(ValidatorNode, OpenApiPathItem?)>
{
    public OpenApiPaths ApiPaths { get; set; }
    public string MockUrlPattern { get; set; }
}

public class UrlPathMatchQueryHandler : IRequestHandler<UrlPathMatchQuery, (ValidatorNode, OpenApiPathItem?)>
{
    public Task<(ValidatorNode, OpenApiPathItem?)> Handle(UrlPathMatchQuery request, CancellationToken cancellationToken)
    {
        OpenApiPathItem? matchingPath = null;

        var regex = new Regex(request.MockUrlPattern);
        foreach (var apiPaths in request.ApiPaths)
        {
            if (regex.Match(apiPaths.Key).Success)
            {
                matchingPath = apiPaths.Value;
                break;
            }
        }

        if (matchingPath is null)
        {
            return Task.FromResult((new ValidatorNode
            {
                Name = request.MockUrlPattern,
                Description = $"Couldn't find matching API Path for Mock {request.MockUrlPattern}",
                Type = ValidatorType.UrlMatch,
                ValidationResult = ValidationResult.Failed
            }, matchingPath));
        }

        return Task.FromResult((new ValidatorNode
        {
            Name = request.MockUrlPattern,
            Type = ValidatorType.UrlMatch,
            ValidationResult = ValidationResult.Passed
        }, matchingPath));
    }
}
