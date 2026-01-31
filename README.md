# Book Fast (Containers)
A sample demonstrating how to implement a containerized multitenant facility management and accommodation booking application.

## Features

### Architecture
- 4 bounded contexts as standalone microservices
- Clean Architecture
- CQRS and DDD (with [reliable domain events](https://dzimchuk.net/reliable-domain-events/))
- ASP.NET Core, EF Core, Blazor, gRPC
- MediatR, FluentValidation, MassTransit
- OpenTelemetry tracing and metrics
- Azure Infrastructure as code (IaC) with Bicep

## Running the applicaton

The application can be run locally in Docker Compose. It can also be deployed to Azure Container Apps or Azure Kubernetes Service (AKS).

### Running locally in Docker Compose

```
docker-compose  -f "docker-compose.yml" -f "docker-compose.development.yml" --no-ansi build
docker-compose  -f "docker-compose.yml" -f "docker-compose.development.yml" --no-ansi up -d --no-build --force-recreate --remove-orphans
```

### Running in Azure Container Apps

TODO

### Running in Kubernetes (AKS)

TODO
