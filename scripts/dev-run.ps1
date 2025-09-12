#!/usr/bin/env pwsh
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$useDocker = $env:USE_DOCKER
if ($useDocker -eq 'true') {
    docker compose -f docker-compose.dev.yml up --build -d
} else {
    dotnet run --project src/Web/Api/CleanTemplate.Api.csproj
}
