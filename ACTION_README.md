# WireMock OpenAPI Validator GitHub Action

Automatically validate WireMock stub mappings against OpenAPI specifications in your GitHub Actions workflows. Perfect for ensuring your mocks stay in sync with your API contracts in PR pipelines.

## Features

- ✅ **Automated Validation**: Validates WireMock stubs against OpenAPI v3 specs
- 💬 **PR Comments**: Posts detailed validation results directly to pull requests
- 📊 **Rich Reporting**: Summary tables, pass rates, and detailed failure information
- 🔄 **Comment Updates**: Updates existing comments on new commits to avoid spam
- ⚡ **Fast Setup**: Composite action with minimal configuration required
- 🎯 **Flexible Options**: Control warnings, output formats, and more

## Usage

### Basic Example

Add this to your GitHub Actions workflow (e.g., `.github/workflows/validate-mocks.yml`):

```yaml
name: Validate WireMock Mappings

on:
  pull_request:
    paths:
      - 'openapi/**'
      - 'wiremock/mappings/**'

jobs:
  validate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Validate WireMock Mappings
        uses: your-org/wiremock-openapi-validator@v1
        with:
          openapi-path: 'openapi/api-spec.yml'
          wiremock-path: 'wiremock/mappings'
```

### With All Options

```yaml
- name: Validate WireMock Mappings
  uses: your-org/wiremock-openapi-validator@v1
  with:
    openapi-path: 'specs/openapi.yml'
    wiremock-path: 'mocks/mappings'
    fail-on-warnings: true
    post-comment: true
    github-token: ${{ secrets.GITHUB_TOKEN }}
```

### Using Outputs

```yaml
- name: Validate WireMock Mappings
  id: validate
  uses: your-org/wiremock-openapi-validator@v1
  with:
    openapi-path: 'openapi.yml'
    wiremock-path: 'mappings'

- name: Check Results
  run: |
    echo "Validation passed: ${{ steps.validate.outputs.validation-passed }}"
    echo "Total checks: ${{ steps.validate.outputs.total-checks }}"
    echo "Passed: ${{ steps.validate.outputs.passed-checks }}"
    echo "Failed: ${{ steps.validate.outputs.failed-checks }}"
    echo "Warnings: ${{ steps.validate.outputs.warning-checks }}"
```

## Inputs

| Input | Description | Required | Default |
|-------|-------------|----------|---------|
| `openapi-path` | Path to the OpenAPI specification file (YAML or JSON) | Yes | - |
| `wiremock-path` | Path to the WireMock mappings directory | Yes | - |
| `fail-on-warnings` | Fail the build if warnings are found | No | `false` |
| `post-comment` | Post validation results as a PR comment | No | `true` |
| `github-token` | GitHub token for posting PR comments | No | `${{ github.token }}` |

## Outputs

| Output | Description |
|--------|-------------|
| `validation-passed` | Whether all validations passed (`true`/`false`) |
| `total-checks` | Total number of validation checks performed |
| `passed-checks` | Number of checks that passed |
| `failed-checks` | Number of checks that failed |
| `warning-checks` | Number of warnings |
| `error-checks` | Number of errors |

## What Gets Validated?

The action validates the following aspects of your WireMock mappings:

### ✅ HTTP Methods
Ensures the HTTP method in your mock matches the OpenAPI operation

### ✅ URL Path Matching
Verifies that mock URL patterns match actual API paths

### ✅ Required Parameters
Checks that all required query/path/header parameters are present

### ✅ Parameter Types
Validates that parameter types match the OpenAPI specification

### ✅ Response Structure
Verifies response properties exist and have correct types

### ✅ Required Response Properties
Ensures all required response fields are present in mocks

## Example PR Comment

The action posts a detailed comment to your PR:

```markdown
## ✅ WireMock OpenAPI Validation Results

**All validations passed!**

### Summary

| Metric | Count |
|--------|-------|
| Total Checks | 45 |
| ✅ Passed | 43 |
| ⚠️ Warnings | 2 |
| ❌ Failed | 0 |
| 🔴 Errors | 0 |
| **Pass Rate** | **95.6%** |
```

## Advanced Usage

### Fail on Warnings

Use `fail-on-warnings: true` to enforce zero warnings in your codebase:

```yaml
- uses: your-org/wiremock-openapi-validator@v1
  with:
    openapi-path: 'api.yml'
    wiremock-path: 'mappings'
    fail-on-warnings: true  # Build fails if any warnings
```

### Disable PR Comments

For CI environments outside pull requests or to disable comments:

```yaml
- uses: your-org/wiremock-openapi-validator@v1
  with:
    openapi-path: 'api.yml'
    wiremock-path: 'mappings'
    post-comment: false
```

### Multiple OpenAPI Specs

Validate against multiple specifications:

```yaml
- name: Validate User API Mocks
  uses: your-org/wiremock-openapi-validator@v1
  with:
    openapi-path: 'specs/users-api.yml'
    wiremock-path: 'mocks/users'

- name: Validate Orders API Mocks
  uses: your-org/wiremock-openapi-validator@v1
  with:
    openapi-path: 'specs/orders-api.yml'
    wiremock-path: 'mocks/orders'
```

### Matrix Strategy

Validate multiple environments:

```yaml
strategy:
  matrix:
    environment: [dev, staging, prod]

steps:
  - uses: actions/checkout@v4

  - name: Validate ${{ matrix.environment }} Mocks
    uses: your-org/wiremock-openapi-validator@v1
    with:
      openapi-path: 'specs/${{ matrix.environment }}/api.yml'
      wiremock-path: 'mocks/${{ matrix.environment }}'
```

## CLI Tool

This action is built on the WireMock OpenAPI Validator CLI tool. You can also use the CLI locally:

```bash
# Install globally
dotnet tool install --global Wiremock.OpenAPIValidator

# Run validation
wiremockopenapi -o openapi.yml -w mappings/

# With options
wiremockopenapi -o api.yml -w mappings/ --format json --quiet
```

### CLI Options

- `--format`: Output format (`console`, `json`, `junit`, `github`)
- `--output-file`: Write results to a file
- `--quiet`: Suppress banner and charts
- `--no-color`: Disable colored output

## Requirements

- .NET 10.0 SDK (automatically installed by the action)
- WireMock mapping files in JSON format
- OpenAPI 3.x specification (YAML or JSON)

## Troubleshooting

### Action fails with "tool not found"

The action automatically installs the tool. If you encounter issues, ensure:
- Your runner has .NET 10.0 SDK available
- The tool installation step completes successfully

### PR comment not posted

Check that:
- The workflow is triggered by a `pull_request` event
- The `github-token` input has permissions to comment on PRs
- `post-comment` is set to `true` (default)

### Validation fails locally but passes in CI

Ensure you're using the same version of the tool:
```bash
dotnet tool update --global Wiremock.OpenAPIValidator
```

## Contributing

Issues and pull requests are welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## License

[MIT License](LICENSE)

---

Built with ❤️ for teams using WireMock and OpenAPI
