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

Azure CLI and Powershell commands that apply ARM templates work seamlessly with Bicep files translating them to ARM templates before sending to Azure.

By default ARM deployments are incremental.

Currently the template is expected to run at resource group scope, thus the target resource group should be created prior to running the template.

```
az deployment group create -g <resource group name> --parameters bookfast.env.<env>.bicepparam
```
