param location string
param solutionName string
param environment string

resource publicIPAddress 'Microsoft.Network/publicIPAddresses@2025-01-01' = {
  name: '${solutionName}-${environment}'
  location: location
  sku: {
    name: 'Standard'
    tier: 'Regional'
  }
  properties: {
    publicIPAddressVersion: 'IPv4'
    publicIPAllocationMethod: 'Static'
    idleTimeoutInMinutes: 4
    ipTags: []
    ddosSettings: {
      protectionMode: 'VirtualNetworkInherited'
    }
  }
}

resource natGateway 'Microsoft.Network/natGateways@2025-01-01' = {
  name: '${solutionName}-${environment}'
  location: location
  sku: {
    name: 'Standard'
  }
  properties: {
    idleTimeoutInMinutes: 4
    publicIpAddresses: [
      {
        id: publicIPAddress.id
      }
    ]
  }
}

output natGatewayId string = natGateway.id
