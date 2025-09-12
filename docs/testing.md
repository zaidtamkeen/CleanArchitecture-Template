# Testing Guide

The solution includes unit tests and end-to-end tests.

## Running Tests
```bash
dotnet test CleanTemplate.sln -c Release --collect:"XPlat Code Coverage" --results-directory ./reports/tests
~/.dotnet/tools/reportgenerator -reports:reports/tests/**/coverage.cobertura.xml -targetdir:reports/coverage -reporttypes:Html
```

Coverage reports are generated under `reports/coverage`.
