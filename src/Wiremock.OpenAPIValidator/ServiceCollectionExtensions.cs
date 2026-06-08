using Microsoft.Extensions.DependencyInjection;
using Wiremock.OpenAPIValidator.Commands;
using Wiremock.OpenAPIValidator.Formatters;
using Wiremock.OpenAPIValidator.Queries;

namespace Wiremock.OpenAPIValidator;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddValidatorServices(this IServiceCollection services)
    {
        // Mediator
        services.AddSingleton<IMediator, SimpleMediator>();

        // Handlers
        services.AddTransient<OpenApiDocumentReaderHandler>();
        services.AddTransient<WiremockMappingsReaderCommandHandler>();
        services.AddTransient<WiremockResponseReaderCommandHandler>();
        services.AddTransient<HttpMethodQueryHandler>();
        services.AddTransient<ParameterTypeQueryHandler>();
        services.AddTransient<ParameterRequiredQueryHandler>();
        services.AddTransient<PropertyTypeQueryHandler>();
        services.AddTransient<PropertyRequiredQueryHandler>();
        services.AddTransient<WireMockMappingsQueryHandler>();
        services.AddTransient<ServiceInfromationQueryHandler>();
        services.AddTransient<UrlPathMatchQueryHandler>();

        // Formatters
        services.AddTransient<ConsoleOutputFormatter>();
        services.AddTransient<JsonOutputFormatter>();
        services.AddTransient<JUnitXmlOutputFormatter>();
        services.AddTransient<GitHubActionsFormatter>();

        // Validation service
        services.AddSingleton<ValidationService>();

        return services;
    }
}
