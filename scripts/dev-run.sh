#!/usr/bin/env bash
set -euo pipefail

USE_DOCKER=${USE_DOCKER:-false}
if [ "$USE_DOCKER" = "true" ]; then
  docker compose -f docker-compose.dev.yml up --build -d
else
  dotnet run --project src/Web/Api/CleanTemplate.Api.csproj
fi
