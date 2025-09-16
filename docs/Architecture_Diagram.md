# Architecture Diagram

This service follows Clean Architecture with ASP.NET Core Web API, Application (CQRS), Domain, and Infrastructure (EF Core) layers. Identity and external services are integrated via HTTP/gRPC.

```mermaid
direction TB
flowchart TB
  user[Client / Consumer] --> api[Web API (ASP.NET Core)]
  api --> app[Application (CQRS / Handlers)]
  app --> dom[Domain (Entities, Interfaces)]
  app --> infra[Infrastructure (EF Core, Repositories, Migrations)]
  infra --> db[(Relational Database)]
  api -.-> id[Identity Service (JWT / Policies / gRPC)]
  api -.-> ext[External Services (e.g., PayMobile)]

  subgraph Web
    api
  end
  subgraph Core
    app --- dom
  end
  subgraph Infrastructure
    infra --- db
  end
```

Notes:
- Controllers are thin; they delegate to Commands/Queries handled in Application.
- Mapster handles DTO mapping; FluentValidation validates inputs.
- Serilog is configured via appsettings; Autofac manages DI.
- Policy-based authorization uses permissions from the Identity service.

