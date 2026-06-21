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

### Install and trust ASP.NET Core HTTPS development certificate

Run the following command to check if the certificate already exists:

```
dotnet dev-certs https --check --trust
```

If so, move on to the next step.

To clean existing certificates and generate and trust a new one:

```
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### Export ASP.NET Core HTTPS development certificate

Docker Compose configuration explicitely references the HTTPS certificate so it can be configured with Kestrel. It is expected to be located under `%APPDATA%/ASP.NET/Https` folder of your user profile and the PFX password should be `test`.

To export your local certificate, run the following command from within your solution folder:

```
dotnet dev-certs https --export-path %APPDATA%/ASP.NET/Https/bookfast.pfx --password test
```

### Docker Compose

```
docker-compose  -f "docker-compose.yml" -f "docker-compose.development.yml" --no-ansi build
docker-compose  -f "docker-compose.yml" -f "docker-compose.development.yml" --no-ansi up -d --no-build --force-recreate --remove-orphans
```

## Deploying to Azure Container Apps

TODO

## Deploying to Kubernetes (AKS)

TODO
