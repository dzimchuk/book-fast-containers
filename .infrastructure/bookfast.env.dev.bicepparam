using 'bookfast.env.bicep'

var sourceKeyVaultSubscriptionId = readEnvironmentVariable('BOOKFAST_SOURCE_KEYVAULT_SUBSCRIPTION_ID')
var sourceKeyVaultResourceGroup = readEnvironmentVariable('BOOKFAST_SOURCE_KEYVAULT_RESOURCE_GROUP', 'Common')
var sourceKeyVaultName = readEnvironmentVariable('BOOKFAST_SOURCE_KEYVAULT_NAME', 'source-keyvault')

param environment = 'dev'

param infrastructureResourceGroup = ''
param containerAppsEnvZoneRedundant = false

param publicNetworkAccess = true

param keyVaultAllowedIpAddresses = []

param sqlServerAllowedIpAddresses = []

param sqlServerAdministratorLogin = az.getSecret(sourceKeyVaultSubscriptionId, sourceKeyVaultResourceGroup, sourceKeyVaultName, 'bookfast-dev-sql-admin-login')
param sqlServerAdministratorPassword = az.getSecret(sourceKeyVaultSubscriptionId, sourceKeyVaultResourceGroup, sourceKeyVaultName, 'bookfast-dev-sql-admin-password')

param identitySigningCertificate = az.getSecret(sourceKeyVaultSubscriptionId, sourceKeyVaultResourceGroup, sourceKeyVaultName, 'bookfast-dev-identity-signing-certificate')
param identityEncryptionKey = az.getSecret(sourceKeyVaultSubscriptionId, sourceKeyVaultResourceGroup, sourceKeyVaultName, 'bookfast-dev-identity-encryption-key')

param sqlServerLocation = 'northeurope'

param searchServiceLocation = 'switzerlandnorth'

param sqlDatabaseParameters = {
  prefix: ''
  sku: {
    name: 'GP_S_Gen5_4'
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: 4
  }
  minCapacity: 2
}

param storageAccountSku = 'Standard_LRS'

param aiFoundryResources = [
  {
    name: 'bookfast-dev'
    location: 'westeurope'
    sku: 'S0'
    models: [
      {
        format: 'OpenAI'
        name: 'text-embedding-3-small'
        version: '1'
        sku: {
          name: 'GlobalStandard'
          capacity: 150
        }
      }
    ]
  }
]
