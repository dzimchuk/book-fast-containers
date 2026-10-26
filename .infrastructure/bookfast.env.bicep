param solutionName string = 'bookfast'

@allowed([
  'dev'
])
param environment string

@description('Unique suffix to preserve certain resource names when moved between subscriptions. Will be auto-generated if not specified.')
param envSuffix string = uniqueString(resourceGroup().id)

@description('Resource group name for Azure Container App insfrastructure services.')
param infrastructureResourceGroup string = ''

@description('Address space of the virtual network.')
param vnetAddressPrefix string = '10.5.0.0/16'

@description('Address prefix of the subnet delegated to the Container Apps environment. Must be at least /27.')
param containersSubnetPrefix string = '10.5.0.0/24'

@description('Address prefix of the subnet hosting private endpoints for supporting services.')
param servicesSubnetPrefix string = '10.5.1.0/24'

@description('A flag to enable zone redundancy for Azure Container Apps Environment.')
param containerAppsEnvZoneRedundant bool = false

@description('Enable or disable public network access for resources. When set to false, resources will be accessible only via private endpoints.')
param publicNetworkAccess bool = false

@description('Public IPv4 addresses/CIDR ranges allowed through the KeyVault firewall (e.g. the Azure DevOps managed pool egress IP). Lets the deploy pipeline read secrets over the data plane without making the whole vault public.')
param keyVaultAllowedIpAddresses array = []

@description('Public IPv4 addresses allowed through the SQL server firewall (e.g. the Azure DevOps managed pool egress IP). Lets the deploy pipeline apply migrations directly.')
param sqlServerAllowedIpAddresses array = []

@secure()
@description('Administrator login username for the SQL server.')
param sqlServerAdministratorLogin string

@secure()
@description('Administrator login password for the SQL server.')
param sqlServerAdministratorPassword string

@description('SQL database SKU and other parameters.')
param sqlDatabaseParameters object

@description('Region for the SQL server and database. Defaults to the resource group location.')
param sqlServerLocation string = ''

@description('Identity signing certificate.')
@secure()
param identitySigningCertificate string

@description('Identity access token encryption key.')
@secure()
param identityEncryptionKey string

@secure()
@description('Azure Communication Services connection string. Optional.')
param communicationServiceConnectionString string = ''

@description('Array of AI Foundry resources and models to deploy')
param aiFoundryResources array = []

@description('Storage Account name, will be generated if not provided.')
param storageAccountName string = ''
@description('Storage Account SKU')
param storageAccountSku string

@description('Service Bus SKU')
param serviceBusSku string = 'Standard'
@description('Zone redundancy for the Service Bus namespace.')
param serviceBusZoneRedundant bool = false

@description('Azure AI Search SKU.')
param searchServiceSku string = 'serverless'

@description('Region for the Azure AI Search service. Defaults to the resource group location.')
param searchServiceLocation string = ''

@description('Blob containers to create.')
param storageContainerNames array = [
  'property-images'
  'data-protection'
]

var rg = resourceGroup()
var location = rg.location

module appInsights 'modules/appInsights.bicep' = {
  name: '${solutionName}-${environment}-appinsights'
  scope: rg
  params: {
    location: location
    solutionName: solutionName
    environment: environment
    keyVaultName: keyVault.outputs.keyVaultName
    connectionStringSecretName: 'appinsights-connection-string'
  }
}

module natGateway 'modules/natGateway.bicep' = {
  name: '${solutionName}-${environment}-natgateway'
  scope: rg
  params: {
    location: location
    solutionName: solutionName
    environment: environment
  }
}

module virtualNetwork 'modules/virtualNetwork.bicep' = {
  name: '${solutionName}-${environment}-vnet'
  scope: rg
  params: {
    location: location
    solutionName: solutionName
    environment: environment
    natGatewayId: natGateway.outputs.natGatewayId
    vnetAddressPrefix: vnetAddressPrefix
    containersSubnetPrefix: containersSubnetPrefix
    servicesSubnetPrefix: servicesSubnetPrefix
  }
}

module keyVault 'modules/keyVault.bicep' = {
  name: '${solutionName}-${environment}-keyvault'
  scope: rg
  params: {
    location: location
    keyVaultName: '${solutionName}-${environment}'
    virtualNetworkId: virtualNetwork.outputs.virtualNetworkId
    virtualNetworkServicesSubnetId: virtualNetwork.outputs.servicesSubnetId
    virtualNetworkContainersSubnetId: virtualNetwork.outputs.containersSubnetId
    publicNetworkAccess: publicNetworkAccess
    keyVaultAllowedIpAddresses: keyVaultAllowedIpAddresses
  }
}

resource secretAccessIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  name: '${solutionName}-${environment}-secret-access'
  location: location
}

module keyVaultAccessPolicy 'modules/keyVaultAccessPolicy.bicep' = {
  name: '${solutionName}-${environment}-keyvaultpolicy'
  scope: rg
  params: {
    keyVaultName: keyVault.outputs.keyVaultName
    principalIds: [ secretAccessIdentity.properties.principalId ]
  }
}

module identitySigningCertificateSecret 'modules/keyVaultSecret.bicep' = {
  name: '${solutionName}-${environment}-identity-signing-certificate'
  scope: rg
  params: {
    keyVaultName: keyVault.outputs.keyVaultName
    secretName: 'identity-signing-certificate'
    secretValue: identitySigningCertificate
    contentType: 'application/x-pkcs12'
  }
}

module identityEncryptionKeySecret 'modules/keyVaultSecret.bicep' = {
  name: '${solutionName}-${environment}-identity-encryption-key'
  scope: rg
  params: {
    keyVaultName: keyVault.outputs.keyVaultName
    secretName: 'identity-encryption-key'
    secretValue: identityEncryptionKey
  }
}

module communicationServiceSecret 'modules/keyVaultSecret.bicep' = if (!empty(communicationServiceConnectionString)) {
  name: '${solutionName}-${environment}-communication-service-connection'
  scope: rg
  params: {
    keyVaultName: keyVault.outputs.keyVaultName
    secretName: 'communication-service-connection-string'
    secretValue: communicationServiceConnectionString
  }
}

module sqlDatabase 'modules/sqlDatabase.bicep' = {
  name: '${solutionName}-sqldb-${environment}'
  scope: rg
  params: {
    location: location
    sqlServerLocation: empty(sqlServerLocation) ? location : sqlServerLocation
    solutionName: solutionName
    environment: environment
    sqlServerAdministratorLogin: sqlServerAdministratorLogin
    sqlServerAdministratorPassword: sqlServerAdministratorPassword
    sqlDatabaseParameters: sqlDatabaseParameters
    virtualNetworkId: virtualNetwork.outputs.virtualNetworkId
    virtualNetworkServicesSubnetId: virtualNetwork.outputs.servicesSubnetId
    keyVaultName: keyVault.outputs.keyVaultName
    connectionStringSecretName: 'sql-connection'
    publicNetworkAccess: publicNetworkAccess
    sqlServerAllowedIpAddresses: sqlServerAllowedIpAddresses
  }
}

module aiFoundry 'modules/aiFoundry.bicep' = {
  name: '${solutionName}-${environment}-aifoundry'
  params: {
    aiFoundryResources: aiFoundryResources
    keyVaultName: keyVault.outputs.keyVaultName
    location: location
    virtualNetworkId: virtualNetwork.outputs.virtualNetworkId
    virtualNetworkServicesSubnetId: virtualNetwork.outputs.servicesSubnetId
    publicNetworkAccess: publicNetworkAccess
  }
}

module storageAccount 'modules/storageAccount.bicep' = {
  name: '${solutionName}-${environment}-storage'
  scope: rg
  params: {
    location: location
    storageAccountName: storageAccountName != '' ? storageAccountName : 'bookfast${envSuffix}'
    storageAccountSku: storageAccountSku
    containerNames: storageContainerNames
    queueNames: []
    fileShareNames: []
    keyVaultName: keyVault.outputs.keyVaultName
    connectionStringSecretName: 'storage-connection-string'
    keySecretName: 'storage-account-key'
    virtualNetworkSubnetId: virtualNetwork.outputs.containersSubnetId
    allowPublicAccess: true // upload via SAS URLs
  }
}

module aiSearch 'modules/aiSearch.bicep' = {
  name: '${solutionName}-${environment}-search'
  scope: rg
  params: {
    location: location
    searchServiceLocation: empty(searchServiceLocation) ? location : searchServiceLocation
    searchServiceName: 'search-${envSuffix}'
    searchServiceSku: searchServiceSku
    virtualNetworkId: virtualNetwork.outputs.virtualNetworkId
    virtualNetworkServicesSubnetId: virtualNetwork.outputs.servicesSubnetId
    publicNetworkAccess: publicNetworkAccess
    keyVaultName: keyVault.outputs.keyVaultName
    adminKeySecretName: 'search-admin-key'
  }
}

module serviceBusNamespace 'modules/serviceBus.bicep' = {
  name: '${solutionName}-${environment}-servicebus-namespace'
  scope: rg
  params: {
    location: location
    serviceBusNamespaceName: 'servicebus-${envSuffix}'
    serviceBusSku: serviceBusSku
    zoneRedundant: serviceBusZoneRedundant
    publicNetworkAccess: false
    keyVaultName: keyVault.outputs.keyVaultName
    connectionStringSecretName: 'servicebus-connection-string'
  }
}

module containerAppsEnvironment 'modules/containerAppsEnvironment.bicep' = {
  name: '${solutionName}-${environment}-containerAppEnv'
  params: {
    location: location
    solutionName: solutionName
    environment: environment
    logAnalyticsWorkspaceId: appInsights.outputs.logAnalyticsResourceId
    virtualNetworkSubnetId: virtualNetwork.outputs.containersSubnetId
    infrastructureResourceGroup: infrastructureResourceGroup
    zoneRedundant: containerAppsEnvZoneRedundant
    fileShareNames: []
    storageAccountName: ''
    storageAccountKey: ''
  }
}

output containerAppsEnvironmentDefaultDomain string = containerAppsEnvironment.outputs.defaultDomain
output keyVaultName string = keyVault.outputs.keyVaultName
output sqlServerName string = sqlDatabase.outputs.sqlServerName
output sqlDatabaseName string = sqlDatabase.outputs.sqlDatabaseName
output storageAccountName string = storageAccount.outputs.name
output serviceBusNamespaceName string = serviceBusNamespace.outputs.name
output searchServiceEndpoint string = aiSearch.outputs.endpoint
