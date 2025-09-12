# Multi-stage Dockerfile for CleanTemplate API
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore CleanTemplate.sln
RUN dotnet publish src/Web/Api/CleanTemplate.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish ./

# create non-root user
RUN useradd -u 1001 appuser && chown -R appuser /app
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "CleanTemplate.Api.dll"]
