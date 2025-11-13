using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Wiremock.OpenAPIValidator
{
    public class Options
    {
        [Option('o', "openApiPath", Required = true, HelpText = "File path to the Open API Spec")]
        public string OpenApiPath { get; set; } = string.Empty;

        [Option('w', "wiremockMappingsPath", Required = true, HelpText = "File path to the stubs mappings folder")]
        public string WiremockMappingsPath { get; set; } = string.Empty;

        [Option('f', "format", Required = false, Default = "console", HelpText = "Output format: console, json, junit, github")]
        public string Format { get; set; } = "console";

        [Option("output-file", Required = false, HelpText = "Write output to file instead of stdout")]
        public string? OutputFile { get; set; }

        [Option("no-color", Required = false, Default = false, HelpText = "Disable colored output")]
        public bool NoColor { get; set; }

        [Option('q', "quiet", Required = false, Default = false, HelpText = "Suppress banner and non-essential output")]
        public bool Quiet { get; set; }
    }

}
