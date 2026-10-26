param aiFoundryResources array = []
param keyVaultName string

param location string
param virtualNetworkId string
param virtualNetworkServicesSubnetId string

param publicNetworkAccess bool

@batchSize(1)
resource cognitiveServicesAccounts 'Microsoft.CognitiveServices/accounts@2025-06-01' = [for account in aiFoundryResources: {
  name: account.name
  location: account.location
  sku: {
    name: account.sku
  }
  kind: 'AIServices'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    apiProperties: {}
    customSubDomainName: account.name
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow'
      virtualNetworkRules: []
      ipRules: []
    }
    allowProjectManagement: true
    publicNetworkAccess: publicNetworkAccess ? 'Enabled' : 'Disabled'
  }
}]

var indexedModels = [for (account, i) in aiFoundryResources: map(account.models, model => {
  accountIndex: i
  sku: model.sku
  name: model.name
  format: model.format
  version: model.version
})]

var models = flatten(indexedModels)

@batchSize(1)
resource deployments 'Microsoft.CognitiveServices/accounts/deployments@2025-06-01' = [for model in models: {
  parent: cognitiveServicesAccounts[model.accountIndex]
  name: model.name
  sku: model.sku
  properties: {
    model: {
      format: model.format
      name: model.name
      version: model.version
    }
    raiPolicyName: 'Microsoft.DefaultV2'
  }
}]

module apiKeys 'keyVaultSecret.bicep' = [for (account, i) in aiFoundryResources: {
  name: '${account.name}-api-key'
  params: {
    keyVaultName: keyVaultName
    secretName: '${account.name}-api-key'
    secretValue: cognitiveServicesAccounts[i].listKeys().key1
  }
}]

var cognitiveServicesDnsZoneName = 'privatelink.cognitiveservices.azure.com'
resource cognitiveServicesDnsZone 'Microsoft.Network/privateDnsZones@2024-06-01' = {
  name: cognitiveServicesDnsZoneName
  location: 'global'
  properties: {}
}

resource cognitiveServicesDnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2024-06-01' = {
  parent: cognitiveServicesDnsZone
  name: '${cognitiveServicesDnsZoneName}-link'
  location: 'global'
  properties: {
    registrationEnabled: false
    resolutionPolicy: 'Default'
    virtualNetwork: {
      id: virtualNetworkId
    }
  }
}

var openaiDnsZoneName = 'privatelink.openai.azure.com'
resource openaiDnsZone 'Microsoft.Network/privateDnsZones@2024-06-01' = {
  name: openaiDnsZoneName
  location: 'global'
  properties: {}
}

resource openaiDnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2024-06-01' = {
  parent: openaiDnsZone
  name: '${openaiDnsZoneName}-link'
  location: 'global'
  properties: {
    registrationEnabled: false
    resolutionPolicy: 'Default'
    virtualNetwork: {
      id: virtualNetworkId
    }
  }
}

var servicesDnsZoneName = 'privatelink.services.ai.azure.com'
resource servicesDnsZone 'Microsoft.Network/privateDnsZones@2024-06-01' = {
  name: servicesDnsZoneName
  location: 'global'
  properties: {}
}

resource servicesDnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2024-06-01' = {
  parent: servicesDnsZone
  name: '${servicesDnsZoneName}-link'
  location: 'global'
  properties: {
    registrationEnabled: false
    resolutionPolicy: 'Default'
    virtualNetwork: {
      id: virtualNetworkId
    }
  }
}

resource privateEndpoints 'Microsoft.Network/privateEndpoints@2024-01-01' = [for (account, i) in aiFoundryResources: {
  name: '${account.name}-private-endpoint'
  location: location
  properties: {
    subnet: {
      id: virtualNetworkServicesSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: '${account.name}-private-endpoint'
        properties: {
          privateLinkServiceId: cognitiveServicesAccounts[i].id
          groupIds: [
            'account'
          ]
        }
      }
    ]
  }
  dependsOn: [
    cognitiveServicesAccounts[i]
    deployments
  ]
}]

resource privateEndpointDnsGroups 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2024-01-01' = [for (account, i) in aiFoundryResources: {
  parent: privateEndpoints[i]
  name: 'default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink-cognitiveservices-azure-com'
        properties: {
          privateDnsZoneId: cognitiveServicesDnsZone.id
        }
      }
      {
        name: 'privatelink-openai-azure-com'
        properties: {
          privateDnsZoneId: openaiDnsZone.id
        }
      }
      {
        name: 'privatelink-services-ai-azure-com'
        properties: {
          privateDnsZoneId: servicesDnsZone.id
        }
      }
    ]
  }
}]
