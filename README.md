# Book Fast
A sample multitenant hotel and accommodation booking platform built with .NET using a services-based architecture.

The application can be run locally in Docker Compose. It can also be deployed to Azure Container Apps.

## Setting up local environment

Prerequisites:

- [Docker](https://www.docker.com/products/docker-desktop/) (with Docker Compose)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

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

The full infrastructure and deployment reference (templates, the master Key Vault, generating the Identity signing certificate, networking and firewall behaviour, database migrations, and the manual post-deployment checklist) lives in [.infrastructure/README.md](.infrastructure/README.md). Setting up the GitHub Actions deployment pipeline is covered in [.github/workflows/README.md](.github/workflows/README.md).

## Configuration

Every service reads its settings from `appsettings.json`, overlaid by `appsettings.Development.json` when running locally in Docker Compose. In Azure the same keys are supplied as container app environment variables using the ASP.NET Core `__` separator (`ConnectionStrings__Sql`, `Authentication__Issuer`, and so on), so the Bicep `env` arrays in `.infrastructure/bookfast.apps.bicep` map one to one onto the JSON.

Secrets are never set as plain environment variables. `bookfast.env.bicep` writes them into the per-environment Key Vault, and the container apps reference them as Key Vault-backed secrets resolved at runtime through the `bookfast-<env>-secret-access` user-assigned identity.

| Concern | Local | Azure |
| --- | --- | --- |
| Database | SQL Server container | Azure SQL Database (single database shared by Identity, PropertyManagement and Booking) |
| Messaging | RabbitMQ (`Messaging:Transport` defaults to `RabbitMq`) | Azure Service Bus (`Messaging__Transport=AzureServiceBus`) |
| Search store | Qdrant | Azure AI Search |
| Embeddings | Ollama (`nomic-embed-text`) | Azure OpenAI (`text-embedding-3-small`, reduced to 768 dimensions) |
| Blob storage | Azurite | Azure Storage, connection string from Key Vault |
| Telemetry | Aspire dashboard over OTLP | Application Insights |
| Token signing | Ephemeral key | Certificate from Key Vault |

Each service exposes `/health/live` (no dependency checks, used as the liveness and startup probe) and `/health/ready` (database connectivity, used as the readiness probe).
