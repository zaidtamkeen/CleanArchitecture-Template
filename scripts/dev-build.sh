#!/usr/bin/env bash
set -euo pipefail

dotnet build CleanTemplate.sln -c Release /m
