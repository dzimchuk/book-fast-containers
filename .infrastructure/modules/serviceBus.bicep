param location string

param serviceBusNamespaceName string

@allowed([
  'Basic'
  'Standard'
  'Premium'
])
param serviceBusSku string

param keyVaultName string
param connectionStringSecretName string

@description('Zone redundancy is only supported on Premium and on Standard in regions with availability zones.')
param zoneRedundant bool

@description('Private endpoints require the Premium SKU.')
param publicNetworkAccess bool
 
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2026-01-01' = {
  name: serviceBusNamespaceName
  location: location
  sku: {
    name: serviceBusSku
  }
  properties: { 
    minimumTlsVersion: '1.2'
    publicNetworkAccess: publicNetworkAccess ? 'Enabled' : 'Disabled'
    disableLocalAuth: false
    zoneRedundant: zoneRedundant
  }
}

var listKeysEndpoint = '${serviceBusNamespace.id}/AuthorizationRules/RootManageSharedAccessKey'

module connection 'keyVaultSecret.bicep' = {
  name: '${serviceBusNamespaceName}-connection'
  params: {
    keyVaultName: keyVaultName
    secretName: connectionStringSecretName
    secretValue: 'Endpoint=sb://${serviceBusNamespace.name}.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=${listKeys(listKeysEndpoint, serviceBusNamespace.apiVersion).primaryKey}'
  }
}

output name string = serviceBusNamespace.name
output id string = serviceBusNamespace.id
