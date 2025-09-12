#!/usr/bin/env bash
set -euo pipefail

dotnet test CleanTemplate.sln -c Release --collect:"XPlat Code Coverage" --results-directory ./reports/tests --logger "trx;LogFileName=test-results.trx"

dotnet tool run reportgenerator "-reports:**/coverage.cobertura.xml" "-targetdir:./reports/coverage" "-reporttypes:HtmlInline_AzurePipelines;Cobertura"

echo "Coverage report generated at ./reports/coverage/index.html"
