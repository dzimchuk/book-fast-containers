param solutionName string = 'bookfast'

@allowed([
  'dev'
])
param environment string

param acrName string = ''
param imageTag string = ''

@description('User assigned identity to be used to pull images from ACR.')
param acrPullIdentity object

@description('CPU and memory for the migrations job replica.')
param resources object = {
  cpu: '0.5'
  memory: '1Gi'
}

@description('Maximum number of seconds a migration run may take before the replica is terminated.')
param replicaTimeout int = 1800

var rg = resourceGroup()
var location = rg.location

resource acrPullIdentityRef 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' existing = {
  scope: resourceGroup(acrPullIdentity.subscriptionId, acrPullIdentity.resourceGroup)
  name: acrPullIdentity.name
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

var defaultDomain = appEnvironment.properties.defaultDomain
var identityUrl = 'https://${solutionName}-identity-${environment}.${defaultDomain}'
var propertyManagementUrl = 'https://${solutionName}-property-management-${environment}.${defaultDomain}'
var searchUrl = 'https://${solutionName}-search-${environment}.${defaultDomain}'
var bookingUrl = 'https://${solutionName}-booking-${environment}.${defaultDomain}'

module migrationsJob 'modules/containerAppJob.bicep' = {
  name: '${solutionName}-${environment}-migrations'
  params: {
    location: location
    solutionName: solutionName
    environment: environment
    jobName: 'migrations'
    acrName: acrName
    image: 'bookfast/migrations:${imageTag}'
    environmentId: appEnvironment.id
    acrPullIdentityId: acrPullIdentityRef.id
    keyVaultAccessIdentityId: secretAccessIdentity.id
    triggerType: 'Manual'
    replicaTimeout: replicaTimeout
    replicaRetryLimit: 0
    resources: resources
    secrets: [
      {
        name: 'sql-connection'
        keyVaultUrl: '${urlPrefix}/sql-connection'
        identity: secretAccessIdentity.id
      }
    ]
    env: [
      {
        name: 'SQL_CONNECTION_STRING'
        secretRef: 'sql-connection'
      }
      {
        name: 'IDENTITY_URL'
        value: identityUrl
      }
      {
        name: 'PROPERTY_MANAGEMENT_URL'
        value: propertyManagementUrl
      }
      {
        name: 'SEARCH_URL'
        value: searchUrl
      }
      {
        name: 'BOOKING_URL'
        value: bookingUrl
      }
    ]
  }
}

output jobName string = migrationsJob.outputs.name
