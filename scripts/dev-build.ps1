#!/usr/bin/env pwsh
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

dotnet build CleanTemplate.sln -c Release /m
