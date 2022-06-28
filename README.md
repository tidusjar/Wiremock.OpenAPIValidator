## WireMock Open API Validator


This tool is intended to test your Wiremock stubs against an OpenAPI (v3) specification. This will check the following

| Type of Check | Description |
| ----------- | ----------- |
| API Method | Ensure the mock is matching the API Verb provided |
| URL Pattern Match | Ensure the mock will actually match the URL |
| Parameters | Checks all required parameters are present and the object type is correct. Will also check optional parameters but will only produce a warning |
| Response | Checks the response object and ensuring the types also match |