# Introduction 
This folder contains [Bicep](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/overview?tabs=bicep) templates for managing infrastructure and deploying the application.

# Prerequisites
- Azure CLI
- Install Bicep modules `az bicep install`
- A resource group for the target environment (templates deploy at resource group scope)
- Secrets provisioned in the master Key Vault (see below)
- Environment variables set for the parameter files (see below)

There are extensions available for VS Code and Visual Studio that make working with Bicep files convenient.

# The master Key Vault

There is a **master Key Vault** (by default named `source-keyvault` in the `Common` resource group) that contains secrets that are not genereted by Bicep modules, e.g. SQL Server admin credentials, Identity signing certificates and encryption keys, external API keys, etc. It is referenced by Azure Resource Manager to inject appropriate values into environment specific KeyVaults and configure services.

Nothing reads the master KeyVault at runtime. The application gets its secrets from environment specific KeyVaults.

## Required configuration on the master Key Vault

1. Select between RBAC (recommended) or 'Vault access policy' on the **Access configuration** page.
2. Enable 'Azure Resource Manager for template deployment' on the same page.
3. Grant the deployment principal the **Key Vault Secrets User** role on the vault, or a `get` secret access policy.

## Required secrets

| Secret | Used for |
| --- | --- |
| `bookfast-<env>-sql-admin-login` | SQL Server administrator login |
| `bookfast-<env>-sql-admin-password` | SQL Server administrator password |
| `bookfast-<env>-identity-signing-certificate` | Base64-encoded PKCS12 used by Identity to sign tokens |
| `bookfast-<env>-identity-encryption-key` | Base64-encoded key used by Identity to encrypt access tokens |
| `bookfast-<env>-communication-service-connection` | *(optional)* Azure Communication Services connection string for outgoing email |

> The SQL administrator password must not contain a `;`. It is embedded in the connection string that the migrations job parses.

## Identity signing certificate and access token encryption key

In Development the Identity service uses ephemeral signing and encryption keys, but in the deployed environment we have to configure the actual certificate and the token encryption key.

The certificate can be generated as described in the [documentation](https://documentation.openiddict.com/configuration/encryption-and-signing-credentials.html#registering-a-certificate-recommended-for-production-ready-scenarios).

```csharp
using var algorithm = RSA.Create(keySizeInBits: 2048);

var subject = new X500DistinguishedName("CN=BookFast Identity Signing Certificate");
var request = new CertificateRequest(subject, algorithm, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, critical: true));

var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(2));

File.WriteAllBytes(@"identity-signing-certificate.pfx", certificate.Export(X509ContentType.Pfx, string.Empty));
```

The generated .pfx file should then be imported into the master Key Vault as `bookfast-<env>-identity-signing-certificate`.

The encryption key is a base-64 encoded symmetrict key that is shared among all backend services. To generate a cryptographically strong 256-bit key use the following procedure:

```
var secretkey = RandomNumberGenerator.GetBytes(32);
var base64String = Convert.ToBase64String(secretkey);
```

Set the generated `base64String` in the master Key Vault as `bookfast-<env>-identity-encryption-key`.

# Environment variables for the parameter files

The parameter files resolve Azure subscription (and optionally other resource names described below) with `readEnvironmentVariable()`.

| Variable | Default | Used by |
| --- | --- | --- |
| `BOOKFAST_SOURCE_KEYVAULT_SUBSCRIPTION_ID` | *(required)* | `bookfast.env.*.bicepparam` |
| `BOOKFAST_SOURCE_KEYVAULT_RESOURCE_GROUP` | `Common` | `bookfast.env.*.bicepparam` |
| `BOOKFAST_SOURCE_KEYVAULT_NAME` | `source-keyvault` | `bookfast.env.*.bicepparam` |
| `BOOKFAST_ACR_PULL_IDENTITY_SUBSCRIPTION_ID` | *(required)* | `bookfast.apps.*.bicepparam`, `bookfast.migrations.*.bicepparam` |
| `BOOKFAST_ACR_PULL_IDENTITY_RESOURCE_GROUP` | `TestContainerRegistry` | as above |
| `BOOKFAST_ACR_PULL_IDENTITY_NAME` | `ACRPullIdentity` | as above |

In GitHub Actions these come from environment-scoped **variables** (not secrets), see the root README.

When running locally:

PowerShell:

```powershell
$env:BOOKFAST_SOURCE_KEYVAULT_SUBSCRIPTION_ID = '<subscription id>'
$env:BOOKFAST_ACR_PULL_IDENTITY_SUBSCRIPTION_ID = '<subscription id>'
```

Bash:

```bash
export BOOKFAST_SOURCE_KEYVAULT_SUBSCRIPTION_ID='<subscription id>'
export BOOKFAST_ACR_PULL_IDENTITY_SUBSCRIPTION_ID='<subscription id>'
```

# Running deployments

Azure CLI and Powershell commands that apply ARM templates work seamlessly with Bicep files translating them to ARM templates before sending to Azure. By default ARM deployments are incremental. The templates deploy at **resource group scope**, so the target resource group must exist beforehand.

The three templates are deployed in a fixed order, because the migrations job has to create the schema before the applications start, and it needs the service URLs to register them as OpenIddict redirect URIs:

```
# 1. infrastructure (only when it changed)
az deployment group create -g <resource group> --parameters bookfast.env.<env>.bicepparam

# 2. import the released images into ACR (GHCR is the build output)
az acr import -n <acr> --source ghcr.io/<owner>/bookfast/identity:<tag> --image bookfast/identity:<tag> --force
#    ... repeat for property-management, search, booking, migrations

# 3. the migrations job definition
az deployment group create -g <resource group> \
    --parameters bookfast.migrations.<env>.bicepparam \
    --parameters acrName=<acr> imageTag=<tag>

# 4. apply migrations and wait for the run to finish
az containerapp job start -g <resource group> -n bookfast-migrations-<env>

# 5. the container apps
az deployment group create -g <resource group> \
    --parameters bookfast.apps.<env>.bicepparam \
    --parameters acrName=<acr> imageTag=<tag>
```

Service URLs are derived from the Container Apps environment default domain, so they are known before the apps are deployed and both templates compute them identically.

## GHCR access for `az acr import`

`az acr import` pulls straight from GitHub Container Registry. If the `bookfast/*` packages are **public**, no credentials are needed.

If they are private, create a token and pass it with `--username`/`--password`:

1. GitHub → Settings → Developer settings → **Personal access tokens**.
2. Either a **classic** token with the `read:packages` scope, or a **fine-grained** token with *Packages: Read-only* permission.
3. Store it as a GitHub Actions **environment secret** named `GHCR_PAT` on the target environment. The deploy workflow only passes credentials when this secret is present.
4. For local imports: `az acr import ... --username <github-username> --password <token>`.

To make the packages public instead: GitHub → your profile/org → Packages → select package → Package settings → Change visibility.

# Database migrations

Migrations run as a **manual-trigger Container App Job** inside the virtual network, not from the pipeline runner. This means no SQL firewall exception is needed for CI, and it works unchanged when `publicNetworkAccess` is turned off.

The job runs the `bookfast/migrations` image, which the build pipeline assembles from:

- `Identity.sql`, `PropertyManagement.sql`, `Booking.sql` (generated by `dotnet ef migrations script -i`)
- `sql/OpenIddictClients.sql` (updates environment specific redirect URIs)

It reads the SQL connection string from Key Vault through the `bookfast-<env>-secret-access` user-assigned identity, and receives the four service URLs as environment variables.

# Resource names and regions

Most resources, including the Key Vault, are named `bookfast-<env>`. Resources with globally unique names use a suffix derived from the resource group ID instead (`envSuffix`, defaulting to `uniqueString(resourceGroup().id)`):

| Resource | Name |
| --- | --- |
| AI Search | `search-<suffix>` |
| Service Bus namespace | `servicebus-<suffix>` |
| Storage account | `bookfast<suffix>` |

All resources are deployed to the resource group's region. AI Search and AI Foundry are the exceptions: they may be deployed elsewhere when the service or model is not available in the main region (`searchServiceParameters.location`, `aiFoundryResources[].location`). Their private endpoints are still deployed in the main region, next to the virtual network.

## Recreating an environment

Key Vaults and AI Foundry accounts are soft-deleted, so deleting the resource group leaves their names reserved. Redeploying fails until the old resources are purged.

**Key Vault** is retained for 90 days by default. If purge protection is not enabled, the vault can be purged right away:

```
az keyvault list-deleted --query "[].{name:name, location:properties.location}" -o table
az keyvault purge -n bookfast-<env>
```

**AI Foundry** accounts are retained for 48 hours. Purging needs the account's original region and resource group:

```
az cognitiveservices account list-deleted -o table
az cognitiveservices account purge -n <account name> -g <resource group> -l <original location>
```

# Networking and firewalls

`publicNetworkAccess` controls whether the supporting services are reachable from the internet. Key Vault, SQL Database, AI Search and AI Foundry all get private endpoints in the `services-subnet` regardless; the flag governs whether the public endpoint stays open alongside them.

`keyVaultAllowedIpAddresses` and `sqlServerAllowedIpAddresses` add firewall exceptions for specific public addresses. They default to empty. The pipeline does not need them because migrations run in-VNet and `az.getSecret()` goes through ARM. Add your own workstation address when you need to query the database or read secrets directly:

```
az deployment group create -g <rg> --parameters bookfast.env.dev.bicepparam \
    --parameters sqlServerAllowedIpAddresses="['<your ip>']"
```

Note that a non-empty allowlist forces the public endpoint to `Enabled`, since IP rules have no effect otherwise. The default network action stays `Deny`.

Two exceptions deliberately remain publicly reachable:

- **Storage account**: PropertyManagement hands SAS upload URLs to browser clients, which have to be able to reach blob storage.
- **Service Bus**: private endpoints require the Premium SKU; the Standard namespace is protected by SAS authentication only.

## Address space

The virtual network CIDRs are parameters on `bookfast.env.bicep`:

| Parameter | Default | Notes |
| --- | --- | --- |
| `vnetAddressPrefix` | `10.5.0.0/16` | |
| `containersSubnetPrefix` | `10.5.0.0/24` | Delegated to `Microsoft.App/environments`; must be at least `/27` |
| `servicesSubnetPrefix` | `10.5.1.0/24` | Private endpoints |

# Manual post-deployment verification

1. `GET https://bookfast-identity-<env>.<default domain>/health/ready` returns `Healthy`, repeat for `property-management`, `search` and `booking`.
2. `GET https://bookfast-identity-<env>.<default domain>/.well-known/openid-configuration` returns a document whose `issuer` matches the configured `Authentication__Issuer`.
3. Swagger UI on each service completes the authorization code + PKCE flow. This confirms `OpenIddictClients.sql` registered the right redirect URIs.
4. Call a PropertyManagement endpoint with a `provider`-scoped token and a Booking endpoint with a `customer`-scoped token, confirming audience wiring.
5. Create a property, upload an image through the returned SAS URL, and confirm the blob lands in the `property-images` container.
6. Confirm the Search service returns the new property, which exercises the Service Bus transport and the consumers end to end.
7. In the portal: Service Bus topics created by MassTransit, the migrations job's last execution is `Succeeded`, and Application Insights is receiving traces from all four services.
