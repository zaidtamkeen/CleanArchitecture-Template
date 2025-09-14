# gRPC Integration with Tamkeen.IdentityService

This document explains how to enable and verify gRPC connectivity between CleanTemplate.Api and Tamkeen.IdentityService using the shared identity.proto contract.

## 1) Packages
- Grpc.Net.Client (Api)
- Google.Protobuf (Api)
- Grpc.Tools (Api, PrivateAssets=All)

These are already added to `src/Web/Api/CleanTemplate.Api.csproj`.

## 2) Proto Linking
The project links the proto directly from the IdentityService repository to avoid duplication:

- Protobuf Include: `..\\..\\..\\Tamkeen.IdentityService\\Protos\\identity.proto`
- GrpcServices: `Client`

## 3) AppSettings
In `src/Web/Api/appsettings.Development.json`:

```
"Identity": {
  "Transport": "Grpc",
  "Authority": "https://localhost:5001", // for JwtBearer validation if needed
  "Audience": "tamkeen.apis",
  "RequireHttpsMetadata": true,
  "Grpc": {
    "Address": "https://localhost:7101",
    "AllowUntrusted": true
  },
  "ClientUser": {
    "TenantId": "default",
    "Username": "<service-account>",
    "Password": "<secret>"
  }
}
```

- `AllowUntrusted=true` is convenient for development; for production set to false.
- Replace the client user credentials with a valid service account provisioned in IdentityService.

## 4) Code Paths
- `External/IdentityTokenProvider.cs` implements both API and gRPC providers.
  - Set `Identity:Transport` to `Grpc` to use the gRPC provider.
  - gRPC provider calls `Identity.Authenticate` and returns the `accessToken` on success.

## 5) Running IdentityService
From repo root:

```
dotnet build -c Release Tamkeen.IdentityService/Tamkeen.IdentityService/Tamkeen.IdentityService.sln
$env:ASPNETCORE_ENVIRONMENT='Development'
dotnet run -c Release --project Tamkeen.IdentityService/Tamkeen.IdentityService/src/Web/Api/Tamkeen.IdentityService.Api.csproj
```

Notes:
- Ensure dev HTTPS certs are trusted: `dotnet dev-certs https --trust`
- Ensure SQL LocalDB is installed; default connection strings are used in dev
- Optionally run Redis locally or adjust `DistributedCacheConfig`

Health endpoint (when running): `https://localhost:7101/health`

## 6) Tests
Run tests:

```
dotnet test -c Release CleanArchitecture-Template/CleanTemplate.sln -v normal
```

- `IdentityGrpcConnectivityTests` calls `ValidateToken` with an invalid token to verify that the gRPC pipeline is reachable. It skips automatically with a clear message if the service is not started.

## 7) Troubleshooting
- If you see TLS errors, re-trust dev certs and ensure antivirus does not intercept `localhost` TLS
- If EF Core migration errors occur in IdentityService, confirm LocalDB is available and the app has permission to create the dev database
- Use `--urls` override when running the IdentityService if 7101 is in use: `dotnet run --urls https://localhost:7102`

