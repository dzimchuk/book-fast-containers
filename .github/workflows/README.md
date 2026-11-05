# Pipelines

Two GitHub Actions workflows build and deploy BookFast. Infrastructure templates and the Azure-side resources they need are described in [.infrastructure/README.md](../../.infrastructure/README.md).

| Workflow | Trigger | Purpose |
| --- | --- | --- |
| [build.yml](build.yml) | push / pull request to `main` or `master`, or manual | Builds and tests the solution, generates idempotent EF Core migration scripts, pushes images to GHCR, and publishes a `deployment-manifest.json` artifact recording whether the build is deployable and which image tag it produced |
| [deploy.yml](deploy.yml) | manual (`workflow_dispatch`) | Resolves a Build run (the latest successful one on the branch by default), verifies the build is deployable, then calls `deploy-environment.yml` |
| [deploy-environment.yml](deploy-environment.yml) | called by `deploy.yml` | Deploys one environment (see below) |

`deploy-environment.yml`:

1. authenticates to Azure with OIDC federated credentials (no stored client secret),
2. optionally deploys the infrastructure template (`bookfast.env.<env>.bicepparam`),
3. imports the released images from GHCR into ACR,
4. deploys and runs the migrations Container App Job, waiting for it to succeed,
5. deploys the four container apps.

The job targets a GitHub **Environment**, which provides the approval gate and scopes the variables and secrets below.

# One-time setup

## 1. Azure identity for GitHub Actions

Create an app registration (or a user-assigned managed identity) that the workflow signs in as, and add a federated credential for each GitHub Environment. No client secret is created.

The job declares `environment: <env>`, so the federated credential **subject must be environment-based**: `repo:<owner>/<repo>:environment:<env>`. A subject for a branch (`ref:refs/heads/main`) will not match and `azure/login` fails with `AADSTS700213`.

```
az ad app create --display-name bookfast-github-deploy --query appId -o tsv   # -> <client id>
az ad sp create --id <client id>
```

Save the following as `credential.json` and register it (repeat per environment):

```json
{
  "name": "bookfast-dev",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:<owner>/<repo>:environment:dev",
  "audiences": [ "api://AzureADTokenExchange" ]
}
```

```
az ad app federated-credential create --id <client id> --parameters credential.json
```

## 2. Azure permissions

Grant the service principal the following roles.

| Role | Scope | Needed for |
| --- | --- | --- |
| **Contributor** | The environment's resource group | Deploying `bookfast.env`, `bookfast.migrations` and `bookfast.apps`, including Key Vault access policies, and starting the migrations job |
| **Container Registry Data Importer and Data Reader** | The Azure Container Registry | `az acr import` (needs `Microsoft.ContainerRegistry/registries/importImage/action`). Not needed separately if the ACR is in the resource group above |
| **Managed Identity Operator** | The ACR pull user-assigned identity (`BOOKFAST_ACR_PULL_IDENTITY_*`) | Assigning that identity to the container apps and the migrations job. Not needed separately if it is in the resource group above |
| **Key Vault Secrets User** | The master Key Vault | `az.getSecret()` reads the SQL credentials, signing certificate and encryption key during template deployment. See [The master Key Vault](../../.infrastructure/README.md#the-master-key-vault) |

Example:

```
az role assignment create --assignee <client id> --role Contributor \
    --scope /subscriptions/<sub>/resourceGroups/<resource group>
az role assignment create --assignee <client id> --role "Container Registry Data Importer and Data Reader" \
    --scope $(az acr show -n <acr> --query id -o tsv)
az role assignment create --assignee <client id> --role "Managed Identity Operator" \
    --scope $(az identity show -g <acr pull identity rg> -n <acr pull identity> --query id -o tsv)
az role assignment create --assignee <client id> --role "Key Vault Secrets User" \
    --scope $(az keyvault show -n <master vault> --query id -o tsv)
```

The resource group must exist beforehand, and the master Key Vault must have **Azure Resource Manager for template deployment** enabled and its secrets provisioned (see [.infrastructure/README.md](../../.infrastructure/README.md)).

## 3. GitHub Environment

Create an Environment named like the `.bicepparam` suffix (for example `dev`) under Settings → Environments, optionally with required reviewers as an approval gate. Add the variables and secrets below to it.

### Variables

| Variable | Purpose |
| --- | --- |
| `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` | Federated credential used by `azure/login` (client id of the identity from step 1) |
| `AZURE_RESOURCE_GROUP` | Target resource group |
| `AZURE_CONTAINER_REGISTRY` | ACR name that images are imported into |
| `BOOKFAST_SOURCE_KEYVAULT_*`, `BOOKFAST_ACR_PULL_IDENTITY_*` | Resolved by `readEnvironmentVariable()` when compiling the parameter files. See [Environment variables for the parameter files](../../.infrastructure/README.md#environment-variables-for-the-parameter-files) for the full list and defaults |

### Secrets

| Secret | Purpose |
| --- | --- |
| `GHCR_PAT` | Only needed if the GHCR packages are private |

## 4. GHCR access

The build pushes images to `ghcr.io/<owner>/bookfast/<service>` using the workflow's `GITHUB_TOKEN`. New packages are **private** by default. The deploy imports them with `az acr import`, which needs credentials only if the packages are private.

Choose one:

- **Make the packages public** (identity, property-management, search, booking, migrations): GitHub → your profile/org → Packages → select package → Package settings → Change visibility. No secret is needed, and deployments don't depend on a personal token.
- **Keep them private** and provide a token:
  1. GitHub → Settings → Developer settings → Personal access tokens → **Tokens (classic)**. Fine-grained tokens cannot be used: they have no Packages permission and GHCR does not accept them.
  2. Generate a classic token with only the `read:packages` scope. If the org enforces SAML SSO, authorize the token for the org.
  3. Store it as a secret named `GHCR_PAT`. Use an **environment secret** to scope it to specific environments (and their approval rules), or a **repository secret** to share one token across all environments. An environment secret overrides a repository secret of the same name. The workflow only passes credentials when the secret is present, and uses `github.actor` as the username.

For a manual import: `az acr import ... --username <github-username> --password <token>`.

# Running a deployment

Actions → **Deploy** → Run workflow. Pick the environment, optionally a Build run id or run number, and whether to also deploy the infrastructure (`bookfast.env`). Enable that on the first deployment of an environment and whenever the infrastructure templates change. The deployed service URLs are listed in the run summary.

# Troubleshooting

| Symptom | Likely cause |
| --- | --- |
| `AADSTS700213` / no matching federated identity record | Federated credential subject does not match `repo:<owner>/<repo>:environment:<env>` |
| `AuthorizationFailed` during a deployment step | Missing role from the table above on the resource group, ACR or ACR pull identity |
| Template deployment fails reading a secret from the master vault | Missing **Key Vault Secrets User**, or ARM template deployment access not enabled on the vault |
| `az acr import` reports unauthorized or not found for the GHCR source | Packages are private and `GHCR_PAT` is missing, expired, or lacks `read:packages` |
