using Spectre.Console;
using Wiremock.OpenAPIValidator.Models;

namespace Wiremock.OpenAPIValidator.Formatters;

public class ConsoleOutputFormatter : IOutputFormatter
{
    public string Format(ValidatorResults results, Options options)
    {
        // Show banner unless quiet mode
        if (!options.Quiet)
        {
            AnsiConsole.Write(new Rule());
            AnsiConsole.Write(new FigletText("Wiremock Open API Validator")
                .Centered()
                .Color(Color.Aquamarine1));
            AnsiConsole.Write(new Rule());
        }

        // Results table
        var table = new Table()
        {
            Title = new TableTitle("Open API Wiremock Results", new Style(Color.Aquamarine1))
        };
        table.AddColumn("Name");
        table.AddColumn("Check Type");
        table.AddColumn("Result");
        table.AddColumn("Reason");

        foreach (var validation in results.Results)
        {
            table.AddRow(
                new Text(validation.Name),
                new Text(validation.Type.ToString()),
                RenderStatus(validation, options.NoColor),
                new Text(validation.Description)
            );
        }

        AnsiConsole.Write(table);

        // Summary bar chart
        if (!options.Quiet)
        {
            AnsiConsole.Write(new BarChart()
                .Label("[green bold underline]Test Results[/]")
                .CenterLabel()
                .AddItem("Passed", results.Results.Count(x => x.ValidationResult == ValidationResult.Passed), Color.Green)
                .AddItem("Warning", results.Results.Count(x => x.ValidationResult == ValidationResult.Warning), Color.Yellow)
                .AddItem("Failed", results.Results.Count(x => x.ValidationResult == ValidationResult.Failed), Color.Red)
                .AddItem("Error", results.Results.Count(x => x.ValidationResult == ValidationResult.Error), Color.DarkRed));

            // Failure breakdown chart
            if (results.Results.Any(x => x.ValidationResult == ValidationResult.Failed))
            {
                var grouped = results.Results.Where(x => x.ValidationResult == ValidationResult.Failed).GroupBy(x => x.Type);
                AnsiConsole.Write(new BarChart()
                   .Label("[red bold underline]Failure Type Breakdown[/]")
                   .CenterLabel()
                   .AddItems(grouped, (item) => new BarChartItem(item.Key.ToString(), item.Count())));
            }

            // Warning breakdown chart
            if (results.Results.Any(x => x.ValidationResult == ValidationResult.Warning))
            {
                var rnd = new Random();
                var grouped = results.Results.Where(x => x.ValidationResult == ValidationResult.Warning).GroupBy(x => x.Type);
                AnsiConsole.Write(new BarChart()
                   .Label("[yellow bold underline]Warning Type Breakdown[/]")
                   .CenterLabel()
                   .AddItems(grouped, (item) => new BarChartItem(item.Key.ToString(), item.Count(), Color.FromInt32(rnd.Next(255)))));
            }
        }

        // Return empty string since output is written directly to console
        return string.Empty;
    }

    private static Markup RenderStatus(ValidatorNode validation, bool noColor)
    {
        if (noColor)
        {
            return new Markup(validation.ValidationResult.ToString());
        }

        return validation.ValidationResult switch
        {
            ValidationResult.Passed => new Markup("[Green]Passed[/]"),
            ValidationResult.Warning => new Markup("[black on yellow]Warning[/]"),
            ValidationResult.Failed => new Markup("[black on red]Failed[/]"),
            ValidationResult.Error => new Markup("[white on darkred]Error[/]"),
            _ => throw new NotImplementedException(),
        };
    }
}
