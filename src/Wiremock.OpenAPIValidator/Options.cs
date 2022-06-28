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
        public string OpenApiPath { get; set; }
        [Option('w', "wiremockMappingsPath", Required = true, HelpText = "File path to the stubs mappings folder")]
        public string WiremockMappingsPath { get; set; }
    }

}
