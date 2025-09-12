#!/usr/bin/env pwsh
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

dotnet restore
if (Test-Path '.config/dotnet-tools.json') {
    dotnet tool restore
} else {
    dotnet tool install dotnet-reportgenerator-globaltool --global
}
