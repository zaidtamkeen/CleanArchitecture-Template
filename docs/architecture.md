# Architecture Overview

This template follows Clean Architecture with separate layers for Domain, Application, Infrastructure and Web. The API layer wires dependencies using dependency injection and exposes REST endpoints with API versioning and Swagger.

## Components
- **Domain**: Entities and repository abstractions.
- **Application**: MediatR handlers, validators and business rules.
- **Infrastructure**: EF Core persistence, repositories and JWT services.
- **Web**: ASP.NET Core API with middleware, security, health checks and OpenTelemetry.

Requests flow from controllers through MediatR pipelines to handlers and persistence. Refer to source for detailed implementation.
