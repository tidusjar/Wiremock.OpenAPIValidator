using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Wiremock.OpenAPIValidator.Commands;
using Wiremock.OpenAPIValidator.Formatters;
using Wiremock.OpenAPIValidator.Queries;

namespace Wiremock.OpenAPIValidator
{
    internal static class Program
    {
        static async Task<int> Main(string[] args)
        {
            Options? parsedOptions = null;
            var parseResult = Parser.Default.ParseArguments<Options>(args);

            parseResult.WithParsed(o => parsedOptions = o);

            if (parsedOptions == null)
            {
                return 2; // Invalid arguments
            }

            // Validate paths (show output unless quiet mode or non-console format)
            var showPathValidation = parsedOptions.Format.ToLower() == "console" && !parsedOptions.Quiet;

            if (!File.Exists(parsedOptions.OpenApiPath))
            {
                if (showPathValidation)
                {
                    AnsiConsole.Write(new Markup($"[red]No File was found at: '{parsedOptions.OpenApiPath}'[/]"));
                }
                else
                {
                    Console.Error.WriteLine($"Error: OpenAPI file not found at '{parsedOptions.OpenApiPath}'");
                }
                return 2;
            }

            if (!Directory.Exists(parsedOptions.WiremockMappingsPath))
            {
                if (showPathValidation)
                {
                    AnsiConsole.Write(new Markup($"[red]No Directory was found at: '{parsedOptions.WiremockMappingsPath}'[/]"));
                }
                else
                {
                    Console.Error.WriteLine($"Error: WireMock mappings directory not found at '{parsedOptions.WiremockMappingsPath}'");
                }
                return 2;
            }

            // Show paths if in console mode and not quiet
            if (showPathValidation)
            {
                AnsiConsole.Write(new Markup($"[green]OpenApiPath: [/]"));
                AnsiConsole.Write(new TextPath(parsedOptions.OpenApiPath));
                AnsiConsole.Write(new Markup("[green]Wiremock Mappings Path: [/]"));
                AnsiConsole.Write(new TextPath(parsedOptions.WiremockMappingsPath));
            }


            var services = new ServiceCollection();

            // Register mediator
            services.AddSingleton<IMediator, SimpleMediator>();

            // Register all handlers
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

            // Register formatters
            services.AddTransient<ConsoleOutputFormatter>();
            services.AddTransient<JsonOutputFormatter>();
            services.AddTransient<JUnitXmlOutputFormatter>();
            services.AddTransient<GitHubActionsFormatter>();

            // Register validation service
            services.AddSingleton<ValidationService>();

            var provider = services.BuildServiceProvider();
            var validationService = provider.GetRequiredService<ValidationService>();

            var validationResult = await validationService.ValidateAsync(parsedOptions.OpenApiPath, parsedOptions.WiremockMappingsPath);

            // Select appropriate formatter
            IOutputFormatter formatter = parsedOptions.Format.ToLower() switch
            {
                "json" => provider.GetRequiredService<JsonOutputFormatter>(),
                "junit" => provider.GetRequiredService<JUnitXmlOutputFormatter>(),
                "github" => provider.GetRequiredService<GitHubActionsFormatter>(),
                "console" or _ => provider.GetRequiredService<ConsoleOutputFormatter>()
            };

            // Format the output
            var output = formatter.Format(validationResult, parsedOptions);

            // Write output to file or console
            if (!string.IsNullOrEmpty(parsedOptions.OutputFile))
            {
                await File.WriteAllTextAsync(parsedOptions.OutputFile, output);
                if (!parsedOptions.Quiet)
                {
                    Console.WriteLine($"Results written to: {parsedOptions.OutputFile}");
                }
            }
            else if (!string.IsNullOrEmpty(output))
            {
                // Only write to console if there's output (console formatter writes directly)
                Console.WriteLine(output);
            }

            // Return exit code based on validation results
            if (validationResult.Results.Any(x => x.ValidationResult == ValidationResult.Failed || x.ValidationResult == ValidationResult.Error))
            {
                return 1;
            }

            return 0;
        }
    }
}