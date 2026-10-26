param solutionName string = 'bookfast'

@allowed([
  'dev'
])
param environment string

param acrName string = ''
param imageTag string = ''

@description('Unique suffix to preserve certain resource names when moved between subscriptions. Will be auto-generated if not specified.')
param envSuffix string = uniqueString(resourceGroup().id)

@description('User assigned identity to be used to pull images from ACR.')
param acrPullIdentity object

@description('Infrastructure settings for Container Apps.')
param containerAppsConfig object

@description('AI Foundry account name.')
param aiFoundryAccountName string = '${solutionName}-${environment}'

var rg = resourceGroup()
var location = rg.location

resource acrPullIdentityRef 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' existing = {
  scope: resourceGroup(acrPullIdentity.subscriptionId, acrPullIdentity.resourceGroup)
  name: acrPullIdentity.name
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: '${solutionName}-${environment}'
}

resource keyVault 'Microsoft.KeyVault/vaults@2024-11-01' existing = {
  name: '${solutionName}-${environment}'
}

resource secretAccessIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' existing = {
  name: '${solutionName}-${environment}-secret-access'
}

resource appEnvironment 'Microsoft.App/managedEnvironments@2025-01-01' existing = {
  name: '${solutionName}-${environment}'
}

var urlPrefix = 'https://${keyVault.name}${az.environment().suffixes.keyvaultDns}/secrets'

var kvSqlConnection = 'sql-connection'
var kvStorageConnection = 'storage-connection-string'
var kvServiceBusConnection = 'servicebus-connection-string'
var kvSearchAdminKey = 'search-admin-key'
var kvAiFoundryApiKey = '${aiFoundryAccountName}-api-key'
var kvIdentitySigningCertificate = 'identity-signing-certificate'
var kvIdentityEncryptionKey = 'identity-encryption-key'
var kvCommunicationServiceConnection = 'communication-service-connection-string'

var searchServiceEndpoint = 'https://search-${envSuffix}.search.windows.net'
var embeddingsEndpoint = 'https://${aiFoundryAccountName}.openai.azure.com/'

var defaultDomain = appEnvironment.properties.defaultDomain
var identityUrl = 'https://${solutionName}-identity-${environment}.${defaultDomain}'

var targetPort = 8080

var probes = [
  {
    type: 'Liveness'
    httpGet: {
      path: '/health/live'
      port: targetPort
      scheme: 'HTTP'
    }
    initialDelaySeconds: 10
    periodSeconds: 30
    timeoutSeconds: 5
    failureThreshold: 3
  }
  {
    type: 'Readiness'
    httpGet: {
      path: '/health/ready'
      port: targetPort
      scheme: 'HTTP'
    }
    initialDelaySeconds: 5
    periodSeconds: 10
    timeoutSeconds: 5
    failureThreshold: 6
  }
  {
    type: 'Startup'
    httpGet: {
      path: '/health/live'
      port: targetPort
      scheme: 'HTTP'
    }
    initialDelaySeconds: 5
    periodSeconds: 5
    timeoutSeconds: 5
    failureThreshold: 30
  }
]

var secretAppInsightsConnection = {
  name: 'appinsights-connection'
  value: appInsights.properties.ConnectionString
}

var secretServiceBusConnection = {
  name: 'servicebus-connection'
  keyVaultUrl: '${urlPrefix}/${kvServiceBusConnection}'
  identity: secretAccessIdentity.id
}

var secretSqlConnection = {
  name: 'sql-connection'
  keyVaultUrl: '${urlPrefix}/${kvSqlConnection}'
  identity: secretAccessIdentity.id
}

var secretStorageConnection = {
  name: 'storage-connection'
  keyVaultUrl: '${urlPrefix}/${kvStorageConnection}'
  identity: secretAccessIdentity.id
}

var secretIdentityEncryptionKey = {
  name: 'identity-encryption-key'
  keyVaultUrl: '${urlPrefix}/${kvIdentityEncryptionKey}'
  identity: secretAccessIdentity.id
}

var commonSecrets = [
  secretAppInsightsConnection
  secretServiceBusConnection
  secretIdentityEncryptionKey
]

var commonEnv = [
  {
    name: 'ASPNETCORE_ENVIRONMENT'
    value: containerAppsConfig.aspNetCoreEnvironment
  }
  {
    name: 'ASPNETCORE_HTTP_PORTS'
    value: '${targetPort}'
  }
  {
    name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED'
    value: 'true'
  }
  {
    name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
    secretRef: 'appinsights-connection'
  }
  {
    name: 'ConnectionStrings__MessageBus'
    secretRef: 'servicebus-connection'
  }
  {
    name: 'Messaging__Transport'
    value: 'AzureServiceBus'
  }
  {
    name: 'Authentication__Swagger__Issuer'
    value: '${identityUrl}/'
  }
]

var resourceServerAuthEnv = [
  {
    name: 'Authentication__Issuer'
    value: '${identityUrl}/'
  }
  {
    name: 'Authentication__ValidIssuers__0'
    value: '${identityUrl}/'
  }
  {
    name: 'Authentication__EncryptionSymmetricKey'
    secretRef: 'identity-encryption-key'
  }
  {
    name: 'Authentication__EnableAccessTokenEncryption'
    value: 'true'
  }
]

var identityMailFromAddress = contains(containerAppsConfig.identity, 'mailFromAddress') ? containerAppsConfig.identity.mailFromAddress : ''

var identitySecrets = concat(commonSecrets, [
  secretSqlConnection
  secretStorageConnection
  {
    name: 'identity-signing-certificate'
    keyVaultUrl: '${urlPrefix}/${kvIdentitySigningCertificate}'
    identity: secretAccessIdentity.id
  }
], empty(identityMailFromAddress) ? [] : [
  {
    name: 'communication-service-connection'
    keyVaultUrl: '${urlPrefix}/${kvCommunicationServiceConnection}'
    identity: secretAccessIdentity.id
  }
])

var identityEnv = concat(commonEnv, [
  {
    name: 'ConnectionStrings__Sql'
    secretRef: 'sql-connection'
  }
  {
    name: 'ConnectionStrings__Storage'
    secretRef: 'storage-connection'
  }
  {
    name: 'AuthServer__SigningCertificate'
    secretRef: 'identity-signing-certificate'
  }
  {
    name: 'AuthServer__EncryptionSymmetricKey'
    secretRef: 'identity-encryption-key'
  }
  {
    name: 'AuthServer__EnableAccessTokenEncryption'
    value: 'true'
  }
], empty(identityMailFromAddress) ? [] : [
  {
    name: 'CommunicationService__ConnectionString'
    secretRef: 'communication-service-connection'
  }
  {
    name: 'CommunicationService__MailFromAddress'
    value: identityMailFromAddress
  }
])

module identityApp 'modules/containerApp.bicep' = {
  name: '${solutionName}-${environment}-identity'
  params: {
    location: location
    solutionName: solutionName
    environment: environment
    appName: 'identity'
    acrName: acrName
    image: 'bookfast/identity:${imageTag}'
    environmentId: appEnvironment.id
    acrPullIdentityId: acrPullIdentityRef.id
    keyVaultAccessIdentityId: secretAccessIdentity.id
    targetPort: targetPort
    customDomainName: containerAppsConfig.identity.customDomainName
    secrets: identitySecrets
    env: identityEnv
    probes: probes
    resources: containerAppsConfig.identity.resources
    scale: containerAppsConfig.identity.scale
  }
}

module propertyManagementApp 'modules/containerApp.bicep' = {
  name: '${solutionName}-${environment}-property-management'
  params: {
    location: location
    solutionName: solutionName
    environment: environment
    appName: 'property-management'
    acrName: acrName
    image: 'bookfast/property-management:${imageTag}'
    environmentId: appEnvironment.id
    acrPullIdentityId: acrPullIdentityRef.id
    keyVaultAccessIdentityId: secretAccessIdentity.id
    targetPort: targetPort
    customDomainName: containerAppsConfig.propertyManagement.customDomainName
    secrets: concat(commonSecrets, [
      secretSqlConnection
      secretStorageConnection
    ])
    env: concat(commonEnv, resourceServerAuthEnv, [
      {
        name: 'ConnectionStrings__Sql'
        secretRef: 'sql-connection'
      }
      {
        name: 'BlobStorage__ConnectionString'
        secretRef: 'storage-connection'
      }
      {
        name: 'BlobStorage__ContainerName'
        value: containerAppsConfig.propertyManagement.blobContainerName
      }
    ])
    probes: probes
    resources: containerAppsConfig.propertyManagement.resources
    scale: containerAppsConfig.propertyManagement.scale
  }
}

module searchApp 'modules/containerApp.bicep' = {
  name: '${solutionName}-${environment}-search'
  params: {
    location: location
    solutionName: solutionName
    environment: environment
    appName: 'search'
    acrName: acrName
    image: 'bookfast/search:${imageTag}'
    environmentId: appEnvironment.id
    acrPullIdentityId: acrPullIdentityRef.id
    keyVaultAccessIdentityId: secretAccessIdentity.id
    targetPort: targetPort
    customDomainName: containerAppsConfig.search.customDomainName
    secrets: concat(commonSecrets, [
      {
        name: 'search-admin-key'
        keyVaultUrl: '${urlPrefix}/${kvSearchAdminKey}'
        identity: secretAccessIdentity.id
      }
      {
        name: 'embeddings-api-key'
        keyVaultUrl: '${urlPrefix}/${kvAiFoundryApiKey}'
        identity: secretAccessIdentity.id
      }
    ])
    env: concat(commonEnv, resourceServerAuthEnv, [
      {
        name: 'Store__Provider'
        value: 'AzureAISearch'
      }
      {
        name: 'Store__AzureAISearch__Endpoint'
        value: searchServiceEndpoint
      }
      {
        name: 'Store__AzureAISearch__ApiKey'
        secretRef: 'search-admin-key'
      }
      {
        name: 'Embeddings__Provider'
        value: 'AzureOpenAI'
      }
      {
        name: 'Embeddings__AzureOpenAI__Endpoint'
        value: embeddingsEndpoint
      }
      {
        name: 'Embeddings__AzureOpenAI__ApiKey'
        secretRef: 'embeddings-api-key'
      }
      {
        name: 'Embeddings__AzureOpenAI__Model'
        value: containerAppsConfig.search.embeddingsModel
      }
      {
        name: 'Purge__Window'
        value: containerAppsConfig.search.purge.window
      }
      {
        name: 'Purge__Interval'
        value: containerAppsConfig.search.purge.interval
      }
    ])
    probes: probes
    resources: containerAppsConfig.search.resources
    scale: containerAppsConfig.search.scale
  }
}

module bookingApp 'modules/containerApp.bicep' = {
  name: '${solutionName}-${environment}-booking'
  params: {
    location: location
    solutionName: solutionName
    environment: environment
    appName: 'booking'
    acrName: acrName
    image: 'bookfast/booking:${imageTag}'
    environmentId: appEnvironment.id
    acrPullIdentityId: acrPullIdentityRef.id
    keyVaultAccessIdentityId: secretAccessIdentity.id
    targetPort: targetPort
    customDomainName: containerAppsConfig.booking.customDomainName
    secrets: concat(commonSecrets, [
      secretSqlConnection
    ])
    env: concat(commonEnv, resourceServerAuthEnv, [
      {
        name: 'ConnectionStrings__Sql'
        secretRef: 'sql-connection'
      }
      {
        name: 'Payments__SettlementDelay'
        value: containerAppsConfig.booking.payments.settlementDelay
      }
      {
        name: 'Payments__SweepInterval'
        value: containerAppsConfig.booking.payments.sweepInterval
      }
      {
        name: 'ReservationExpiration__SweepInterval'
        value: containerAppsConfig.booking.reservationExpiration.sweepInterval
      }
    ])
    probes: probes
    resources: containerAppsConfig.booking.resources
    scale: containerAppsConfig.booking.scale
  }
}

output identityUrl string = identityApp.outputs.url
output propertyManagementUrl string = propertyManagementApp.outputs.url
output searchUrl string = searchApp.outputs.url
output bookingUrl string = bookingApp.outputs.url
