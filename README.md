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

The application can be run locally in Docker Compose. It can also be deployed to Azure Container Apps.

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

The full infrastructure and deployment reference (templates, the master Key Vault, generating the Identity signing certificate, networking and firewall behaviour, database migrations, and the manual post-deployment checklist) lives in [.infrastructure/README.md](.infrastructure/README.md).

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

## Pipelines

### Build

[.github/workflows/build.yml](.github/workflows/build.yml) builds and tests the solution, generates idempotent EF Core migration scripts, pushes images to GHCR, and publishes a `deployment-manifest.json` artifact recording whether the build is deployable and which image tag it produced.

### Deploy

[.github/workflows/deploy.yml](.github/workflows/deploy.yml) is triggered manually. It resolves a Build run (the latest successful one by default), verifies the manifest says the build is deployable, then hands over to the reusable [.github/workflows/deploy-environment.yml](.github/workflows/deploy-environment.yml), which:

1. authenticates to Azure with OIDC federated credentials (no stored client secret),
2. optionally deploys the infrastructure template,
3. imports the released images from GHCR into ACR,
4. deploys and runs the migrations Container App Job, waiting for it to succeed,
5. deploys the four container apps.

The job targets a GitHub **Environment**, which provides the approval gate and scopes the configuration below.

#### Environment variables and secrets

Configure these on the GitHub Environment (Settings → Environments → `<env>`):

| Variable | Purpose |
| --- | --- |
| `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` | Federated credential used by `azure/login` |
| `AZURE_RESOURCE_GROUP` | Target resource group |
| `AZURE_CONTAINER_REGISTRY` | ACR name that images are imported into |
| `BOOKFAST_SOURCE_KEYVAULT_*`, `BOOKFAST_ACR_PULL_IDENTITY_*` | Resolved by `readEnvironmentVariable()` when compiling the parameter files |

| Secret | Purpose |
| --- | --- |
| `GHCR_PAT` | Only needed if the GHCR packages are private |

See [.infrastructure/README.md](.infrastructure/README.md) for how to set up the federated credential's Azure permissions, the master Key Vault and its secrets, and the PAT.
