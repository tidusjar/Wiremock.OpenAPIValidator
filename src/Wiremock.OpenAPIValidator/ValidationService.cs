using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.OpenApi.Models;
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

        var openApiInfo = await _mediator.Send<OpenApiDocument>(new OpenApiDocumentReaderCommand
        {
            OpenApiDocLocation = openApiUrl
        });
        var wireMockFiles = await _mediator.Send<string[]>(new WireMockMappingsQuery
        {
            MappingsPath = wireMockMappings
        });
        var paths = openApiInfo.Paths;

        foreach (var mock in wireMockFiles)
        {
            var parsedFile = await _mediator.Send<WiremockMappings?>(new WiremockMappingsReaderCommand
            {
                WiremockMappingPath = mock
            });

            if (parsedFile == null || parsedFile.Mappings.Count is 0)
            {
                continue;
            }

            foreach (var mapping in parsedFile.Mappings)
            {

                if (mapping.Request == null || mapping.Response == null)
                {
                    continue;
                }

                // Path Match Check
                var results = await _mediator.Send<UrlPathMatchResult>(new UrlPathMatchQuery
                {
                    ApiPaths = paths,
                    MockUrlPattern = mapping.Request.UrlPattern!
                });
                validation.Results.Add(results.ValidationNode);
                var matchedPath = results.MatchedPath;
                if (matchedPath == null)
                {
                    continue;
                }

                var operation = matchedPath.Operations.First().Value;

                // Check HTTP Method
                validation.Results.Add(await _mediator.Send<ValidatorNode>(new HttpMethodQuery
                {
                    Api = matchedPath,
                    RequestMethod = mapping.Request.Method,
                    Name = $"{operation.OperationId} - {mapping.Request.Method}",
                }));

                // Check each query parameter
                foreach (var param in operation.Parameters)
                {
                    // Check that it's present if required
                    validation.Results.Add(await _mediator.Send<ValidatorNode>(new ParameterRequiredQuery
                    {
                        Name = $"{operation.OperationId} - {param.Name}",
                        MockedParameters = mapping.Request.QueryParameters,
                        Param = param
                    }));

                    // Ensure Param Type is correct
                    validation.Results.Add(await _mediator.Send<ValidatorNode>(new ParameterTypeQuery
                    {
                        Name = $"{operation.OperationId} - {param.Name}",
                        MockedParameters = mapping.Request.QueryParameters,
                        Param = param
                    }));
                }

                var mockedResponse = await _mediator.Send<Models.WiremockResponseProperties>(new WiremockResponseReaderCommand
                {
                    MockResponseFileName = mapping.Response.FileName,
                    WiremockMappingPath = wireMockMappings
                });

                // Response Property Check
                validation.Results.AddRange(await _mediator.Send<List<ValidatorNode>>(new PropertyRequiredQuery
                {
                    MockProperties = mockedResponse,
                    Name = operation.OperationId,
                    Responses = operation.Responses
                }));

                // Response Property Type
                validation.Results.AddRange(await _mediator.Send<List<ValidatorNode>>(new PropertyTypeQuery
                {
                    MockProperties = mockedResponse,
                    Name = operation.OperationId,
                    Responses = operation.Responses
                }));


            }
        }

        return validation;
    }
}
