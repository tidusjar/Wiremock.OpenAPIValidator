using MediatR;
using System.Text.Json;
using System.Text.Json.Nodes;
using Wiremock.OpenAPIValidator.Commands;
using Wiremock.OpenAPIValidator.Queries;

namespace Wiremock.OpenAPIValidator;

public class ValidationService
{
    private readonly IMediator _mediator;

    public ValidationService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<ValidatorResults> ValidateAsync(string openApiUrl, string wireMockMappings)
    {
        var validation = new ValidatorResults();

        var openApiInfo = await _mediator.Send(new OpenApiDocumentReaderCommand
        {
            OpenApiDocLocation = openApiUrl
        });
        var wireMockFiles = await _mediator.Send(new WireMockMappingsQuery
        {
            MappingsPath = wireMockMappings
        });
        var paths = openApiInfo.Paths;

        foreach (var mock in wireMockFiles)
        {
            var mappings = await _mediator.Send(new WiremockMappingsReaderCommand
            {
                WiremockMappingPath = mock
            });

            if (mappings?.Request == null || mappings?.Response == null)
            {
                continue;
            }

            // Path Match Check
            var results = await _mediator.Send(new UrlPathMatchQuery
            {
                ApiPaths = paths,
                MockUrlPattern = mappings.Request.UrlPattern
            });
            validation.Results.Add(results.Item1);
            var matchedPath = results.Item2;
            if (matchedPath == null)
            {
                continue;
            }

            var operation = matchedPath.Operations.First().Value;

            // Check HTTP Method
            validation.Results.Add(await _mediator.Send(new HttpMethodQuery
            {
                Api = matchedPath,
                RequestMethod = mappings.Request.Method,
                Name = $"{operation.OperationId} - {mappings.Request.Method}",
            }));

            // Check each query parameter
            foreach (var param in operation.Parameters)
            {
                // Check that it's present if required
                validation.Results.Add(await _mediator.Send(new ParameterRequiredQuery
                {
                    Name = $"{operation.OperationId} - {param.Name}",
                    MockedParameters = (JsonElement)mappings.Request.QueryParameters,
                    Param = param
                }));

                // Ensure Param Type is correct
                validation.Results.Add(await _mediator.Send(new ParameterTypeQuery
                {
                    Name = $"{operation.OperationId} - {param.Name}",
                    MockedParameters = (JsonElement)mappings.Request.QueryParameters,
                    Param = param
                }));
            }

            var mockedResponse = await _mediator.Send(new WiremockResponseReaderCommand
            {
                MockResponseFileName = mappings.Response.FileName,
                WiremockMappingPath = wireMockMappings
            });

            // Response Property Check
            validation.Results.AddRange(await _mediator.Send(new PropertyRequiredQuery
            {
                MockProperties = mockedResponse,
                Name = operation.OperationId,
                Responses = operation.Responses
            }));
        }

        return validation;
    }
}
