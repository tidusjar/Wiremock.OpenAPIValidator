using MediatR;
using Microsoft.OpenApi.Models;
using Moq;
using Moq.AutoMock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Wiremock.OpenAPIValidator.Commands;
using Wiremock.OpenAPIValidator.Queries;

namespace Wiremock.OpenAPIValidator.Tests
{
    public class ValidationServiceTests
    {
        private ValidationService _service;
        private AutoMocker _mocker;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();
            _service = _mocker.CreateInstance<ValidationService>();
        }

        [Test]
        public async Task SuccessfulValidation()
        {
            _mocker.Setup<IMediator, Task<OpenApiDocument>>(x => x.Send(It.IsAny<OpenApiDocumentReaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OpenApiDocument
                {
                    Paths = new OpenApiPaths
                    {
                        { "/api/v1/path", new OpenApiPathItem() }
                    }
                });

            _mocker.Setup<IMediator, Task<string[]>>(x => x.Send(It.IsAny<WireMockMappingsQuery>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new[] { "" });

            var mockedParam = "{ \"Param2\": { \"equalTo\": \"All\" } }";
            var doc = JsonDocument.Parse(mockedParam);
            _mocker.Setup<IMediator, Task<WiremockMappings>>(x => x.Send(It.IsAny<WiremockMappingsReaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new WiremockMappings
                {
                    Request = new WiremockRequest
                    {
                        UrlPattern = "abc",
                        QueryParameters = doc.RootElement
                    }
                });

            _mocker.Setup<IMediator, Task<(ValidatorNode, OpenApiPathItem)>>(x => x.Send(It.IsAny<UrlPathMatchQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((new ValidatorNode(), new OpenApiPathItem
                {
                    Operations = new Dictionary<OperationType, OpenApiOperation> { { OperationType.Put, new OpenApiOperation {
                        OperationId = "UnitTest",
                        Parameters = new List<OpenApiParameter> { new OpenApiParameter {  Required = true, Name = "TestParam"} }
                    } } }
                }));

            var result = await _service.ValidateAsync("test1", "test2");
        }
    }
}
