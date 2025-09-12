#!/usr/bin/env pwsh
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

dotnet test CleanTemplate.sln -c Release --collect:"XPlat Code Coverage" --results-directory ./reports/tests --logger "trx;LogFileName=test-results.trx"

dotnet tool run reportgenerator "-reports:**/coverage.cobertura.xml" "-targetdir:./reports/coverage" "-reporttypes:HtmlInline_AzurePipelines;Cobertura"

Write-Output "Coverage report generated at ./reports/coverage/index.html"
