param location string

@description('Region for the Search service. Defaults to the main location; override when the service is not available there. The private endpoint stays in the main location, next to the virtual network.')
param searchServiceLocation string = location

param searchServiceName string

@allowed([
  'basic'
  'standard'
  'standard2'
  'standard3'
  'serverless'
])
param searchServiceSku string = 'basic'

@description('Ignored for the serverless SKU.')
param replicaCount int = 1
@description('Ignored for the serverless SKU.')
param partitionCount int = 1

param virtualNetworkId string
param virtualNetworkServicesSubnetId string

param publicNetworkAccess bool

param keyVaultName string
param adminKeySecretName string

resource searchService 'Microsoft.Search/searchServices@2026-09-01-preview' = {
  name: searchServiceName
  location: searchServiceLocation
  sku: {
    name: searchServiceSku
  }
  properties: {
    replicaCount: searchServiceSku == 'serverless' ? null : replicaCount
    partitionCount: searchServiceSku == 'serverless' ? null : partitionCount
    hostingMode: 'Default'
    // the Search service is reached with an admin key, so key-based auth has to stay enabled
    disableLocalAuth: false
    publicNetworkAccess: publicNetworkAccess ? 'enabled' : 'disabled'
    networkRuleSet: {
      ipRules: []
      bypass: 'AzureServices'
    }
  }
}

var privateEndpointName = 'search-private-endpoint'
resource privateEndpoint 'Microsoft.Network/privateEndpoints@2024-05-01' = {
  name: privateEndpointName
  location: location
  properties: {
    subnet: {
      id: virtualNetworkServicesSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: privateEndpointName
        properties: {
          privateLinkServiceId: searchService.id
          groupIds: [
            'searchService'
          ]
        }
      }
    ]
  }
}

var dnsZoneName = 'privatelink.search.windows.net'
resource dnsZone 'Microsoft.Network/privateDnsZones@2024-06-01' = {
  name: dnsZoneName
  location: 'global'
  properties: {}
}

resource dnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2024-06-01' = {
  parent: dnsZone
  name: '${dnsZoneName}-link'
  location: 'global'
  properties: {
    registrationEnabled: false
    resolutionPolicy: 'Default'
    virtualNetwork: {
      id: virtualNetworkId
    }
  }
}

resource privateEndpointDnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2024-01-01' = {
  parent: privateEndpoint
  name: 'default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: dnsZoneName
        properties: {
          privateDnsZoneId: dnsZone.id
        }
      }
    ]
  }
}

module adminKey 'keyVaultSecret.bicep' = {
  name: '${searchServiceName}-admin-key'
  params: {
    keyVaultName: keyVaultName
    secretName: adminKeySecretName
    secretValue: searchService.listAdminKeys().primaryKey
  }
}

output name string = searchService.name
output endpoint string = 'https://${searchService.name}.search.windows.net'
