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
using Wiremock.OpenAPIValidator.Models;
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
            

            _mocker.Setup<IMediator, Task<OpenApiDocument>>(x => x.Send<OpenApiDocument>(It.IsAny<OpenApiDocumentReaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OpenApiDocument
                {
                    Paths = new OpenApiPaths
                    {
                        { "/api/v1/path", new OpenApiPathItem() }
                    }
                });

            _mocker.Setup<IMediator, Task<string[]>>(x => x.Send<string[]>(It.IsAny<WireMockMappingsQuery>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new[] { "" });

            var mockedParam = "{ \"Param2\": { \"equalTo\": \"All\" } }";
            var doc = JsonDocument.Parse(mockedParam);
            var parsedMappings = new WiremockMappings
            {
                Mappings = new List<WiremockMapping>()
                    {
                        new WiremockMapping()
                        {
                            Request = new WiremockRequest
                            {
                                UrlPattern = "abc",
                                QueryParameters = mockedParam
                            },
                            Response = new WiremockResponse
                            {
                                FileName = "Test1"
                            }
                        },
                        new WiremockMapping()
                        {
                            Request = new WiremockRequest
                            {
                                UrlPattern = "def",
                                QueryParameters = mockedParam
                            },
                            Response = new WiremockResponse
                            {
                                FileName = "Test2"
                            }
                        },
                        new WiremockMapping()
                        {
                            Request = new WiremockRequest
                            {
                                UrlPattern = "ghi",
                                QueryParameters = mockedParam
                            },
                            Response = new WiremockResponse
                            {
                                FileName = "Test3"
                            }
                        }
                    }
            };
            _mocker.Setup<IMediator, Task<WiremockMappings?>>(x => x.Send<WiremockMappings?>(It.IsAny<WiremockMappingsReaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(parsedMappings);

            _mocker.Setup<IMediator, Task<UrlPathMatchResult>>(x => x.Send<UrlPathMatchResult>(It.IsAny<UrlPathMatchQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UrlPathMatchResult
                {
                    ValidationNode = new ValidatorNode(),
                    MatchedPath = new OpenApiPathItem
                    {
                        Operations = new Dictionary<OperationType, OpenApiOperation> { { OperationType.Put, new OpenApiOperation {
                            OperationId = "UnitTest",
                            Parameters = new List<OpenApiParameter> { new OpenApiParameter {  Required = true, Name = "TestParam"} }
                        } } }
                    }
                });

            _mocker.Setup<IMediator, Task<WiremockResponseProperties>>(x => x.Send<WiremockResponseProperties>(It.IsAny<WiremockResponseReaderCommand>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new WiremockResponseProperties());

            _mocker.Setup<IMediator, Task<List<ValidatorNode>>>(x => x.Send<List<ValidatorNode>>(It.IsAny<PropertyRequiredQuery>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new List<ValidatorNode>());

            _mocker.Setup<IMediator, Task<List<ValidatorNode>>>(x => x.Send<List<ValidatorNode>>(It.IsAny<PropertyTypeQuery>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new List<ValidatorNode>());

            var result = await _service.ValidateAsync("test1", "test2");

            Assert.That(result, Is.Not.Null);

            var mediatorMock = _mocker.GetMock<IMediator>();

            mediatorMock.Verify(x => x.Send<OpenApiDocument>(It.IsAny<OpenApiDocumentReaderCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            mediatorMock.Verify(x => x.Send<string[]>(It.IsAny<WireMockMappingsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            mediatorMock.Verify(x => x.Send<WiremockMappings?>(It.IsAny<WiremockMappingsReaderCommand>(), It.IsAny<CancellationToken>()), Times.Once);

            mediatorMock.Verify(x => x.Send<UrlPathMatchResult>(It.IsAny<UrlPathMatchQuery>(), It.IsAny<CancellationToken>()), Times.Exactly(parsedMappings.Mappings.Count));
            mediatorMock.Verify(x => x.Send<ValidatorNode>(It.IsAny<HttpMethodQuery>(), It.IsAny<CancellationToken>()), Times.Exactly(parsedMappings.Mappings.Count));
            mediatorMock.Verify(x => x.Send<ValidatorNode>(It.IsAny<ParameterRequiredQuery>(), It.IsAny<CancellationToken>()), Times.Exactly(parsedMappings.Mappings.Count));
            mediatorMock.Verify(x => x.Send<ValidatorNode>(It.IsAny<ParameterTypeQuery>(), It.IsAny<CancellationToken>()), Times.Exactly(parsedMappings.Mappings.Count));
            mediatorMock.Verify(x => x.Send<WiremockResponseProperties>(It.IsAny<WiremockResponseReaderCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(parsedMappings.Mappings.Count));
            mediatorMock.Verify(x => x.Send<List<ValidatorNode>>(It.IsAny<PropertyRequiredQuery>(), It.IsAny<CancellationToken>()), Times.Exactly(parsedMappings.Mappings.Count));
            mediatorMock.Verify(x => x.Send<List<ValidatorNode>>(It.IsAny<PropertyTypeQuery>(), It.IsAny<CancellationToken>()), Times.Exactly(parsedMappings.Mappings.Count));
        }
    }
}
