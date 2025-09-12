#!/usr/bin/env bash
set -euo pipefail

dotnet restore
if [ -f .config/dotnet-tools.json ]; then
  dotnet tool restore
else
  dotnet tool install dotnet-reportgenerator-globaltool --global
fi
