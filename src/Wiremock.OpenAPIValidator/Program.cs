using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Wiremock.OpenAPIValidator
{
    internal partial class Program
    {
        static async Task Main(string[] args)
        {
            AnsiConsole.Write(new FigletText("Wiremock Open API Validator")
                .Centered()
                .Color(Color.Aquamarine1));
            var rule = new Rule();
            AnsiConsole.Write(rule);
            IServiceCollection services = new ServiceCollection();
            services.AddMediatR(typeof(Program));
            services.AddSingleton<ValidationService>();

            var openApiUrl = @"C:\git\signatureanalytics-data\src\.api-client-config\openapi.yaml";
            var wireMockMappings = @"C:\git\exclaimercloud-ui\build\mocks\signature-analytics\stubs\mappings";

            var provider = services.BuildServiceProvider();
            var validationService = provider.GetRequiredService<ValidationService>();

            var validationResult = await validationService.ValidateAsync(openApiUrl, wireMockMappings);

            var parentWiremock = Directory.GetParent(wireMockMappings);

            var table = new Table()
            {
                Title = new TableTitle("Open API Wiremock Results", new Style(Color.Aquamarine1))
            };
            table.AddColumn("Name");
            table.AddColumn("Check Type");
            table.AddColumn("Failed");
            table.AddColumn("Failure Reason");

            foreach (var validation in validationResult.Results)
            {
                table.AddRow(new Text(validation.Name), new Text(validation.Type.ToString()), RenderStatus(validation), new Text(validation.Description));
            }

            AnsiConsole.Write(table);

            AnsiConsole.Write(new BarChart()
                .Label("[green bold underline]Test Results[/]")
                .CenterLabel()
                .AddItem("Passed", validationResult.Results.Count(x => x.ValidationResult == ValidationResult.Passed), Color.Green)
                .AddItem("Warning", validationResult.Results.Count(x => x.ValidationResult == ValidationResult.Warning), Color.Yellow)
                .AddItem("Failed", validationResult.Results.Count(x => x.ValidationResult == ValidationResult.Failed), Color.Red)
                .AddItem("Error", validationResult.Results.Count(x => x.ValidationResult == ValidationResult.Error), Color.DarkRed));

            Console.ReadLine();


            //using var responseStream = File.OpenRead(Path.Combine(parentWiremock.FullName, "__files", mappings.Response.FileName));
            //var doc = JsonDocument.Parse(responseStream);
            //if (doc.RootElement.ValueKind == JsonValueKind.Object)
            //{
            //    var responseObjects = doc.Deserialize<JsonObject>();
            //}
            //else if (doc.RootElement.ValueKind == JsonValueKind.Array)
            //{
            //    var responseObjects = doc.Deserialize<JsonArray>();
            //}
        }

        private static Markup RenderStatus(ValidatorNode validation) =>
            validation.ValidationResult switch
            {
                ValidationResult.Passed => new Markup("[Green]Passed[/]"),
                ValidationResult.Warning => new Markup("[black on yellow]Warning[/]"),
                ValidationResult.Failed => new Markup("[black on red]Failed[/]"),
                ValidationResult.Error => new Markup("[white on darkred]Error[/]"),
                _ => throw new NotImplementedException(),
            };
    }
}