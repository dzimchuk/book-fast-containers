param location string
param solutionName string
param environment string

param natGatewayId string

@description('Address space of the virtual network.')
param vnetAddressPrefix string = '10.5.0.0/16'

@description('Address prefix of the subnet delegated to the Container Apps environment. Must be at least /27 for workload profile environments.')
param containersSubnetPrefix string = '10.5.0.0/24'

@description('Address prefix of the subnet hosting private endpoints for supporting services.')
param servicesSubnetPrefix string = '10.5.1.0/24'

resource virtualNetwork 'Microsoft.Network/virtualNetworks@2024-07-01' = {
  name: '${solutionName}-${environment}'
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        vnetAddressPrefix
      ]
    }
    encryption: {
      enabled: false
      enforcement: 'AllowUnencrypted'
    }
    subnets: [
      {
        name: 'containers-subnet'
        properties: {
          addressPrefix: containersSubnetPrefix
          natGateway: {
            id: natGatewayId
          }
          serviceEndpoints: [
            {
              service: 'Microsoft.KeyVault'
              locations: [
                '*'
              ]
            }
            {
              service: 'Microsoft.Storage'
              locations: [
                location
              ]
            }
            {
              service: 'Microsoft.ContainerRegistry'
              locations: [
                '*'
              ]
            }
            {
              service: 'Microsoft.ServiceBus'
              locations: [
                '*'
              ]
            }
          ]
          delegations: [
            {
              name: 'Microsoft.App.environments'
              properties: {
                serviceName: 'Microsoft.App/environments'
              }
              type: 'Microsoft.Network/virtualNetworks/subnets/delegations'
            }
          ]
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
          defaultOutboundAccess: true
        }
      }
      {
        name: 'services-subnet'
        properties: {
          addressPrefix: servicesSubnetPrefix
          serviceEndpoints: []
          delegations: []
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
          defaultOutboundAccess: false
        }
      }
    ]
  }
}

output virtualNetworkId string = virtualNetwork.id
output containersSubnetId string = virtualNetwork.properties.subnets[0].id
output servicesSubnetId string = virtualNetwork.properties.subnets[1].id
