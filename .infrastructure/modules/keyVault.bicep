param location string
param keyVaultName string

param virtualNetworkId string
param virtualNetworkServicesSubnetId string

param virtualNetworkContainersSubnetId string

param publicNetworkAccess bool

@description('Public IPv4 addresses/CIDR ranges allowed through the KeyVault firewall (e.g. to be used by CI/CD pipelines).')
param keyVaultAllowedIpAddresses array = []

resource keyVault 'Microsoft.KeyVault/vaults@2024-11-01' = {
  name: keyVaultName
  location: location
  properties: {
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: true
    tenantId: subscription().tenantId
    accessPolicies: []
    sku: {
      name: 'standard'
      family: 'A'
    }
    networkAcls: {
      defaultAction: 'Deny'
      bypass: 'AzureServices'
      virtualNetworkRules: publicNetworkAccess ? [
        {
          id: virtualNetworkContainersSubnetId
          ignoreMissingVnetServiceEndpoint: false
        }
      ] : []
      ipRules: [for ip in keyVaultAllowedIpAddresses: { value: ip }]
    }
    // ipRules only take effect when public network access is Enabled, so a non-empty allowlist
    // forces Enabled (still locked down by the Deny default)
    publicNetworkAccess: (publicNetworkAccess || !empty(keyVaultAllowedIpAddresses)) ? 'Enabled' : 'Disabled'
  }
}

var privateEndpointName = 'keyvault-private-endpoint'
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
          privateLinkServiceId: keyVault.id
          groupIds: [
            'vault'
          ]
        }
      }
    ]
  }
}

var dnsZoneName = 'privatelink.vaultcore.azure.net'
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

output keyVaultName string = keyVault.name
