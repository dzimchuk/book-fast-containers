param location string
param solutionName string
param environment string

param logAnalyticsWorkspaceId string
param virtualNetworkSubnetId string

param infrastructureResourceGroup string = ''

param zoneRedundant bool

param fileShareNames array

param storageAccountName string

@secure()
param storageAccountKey string

resource appEnvironment 'Microsoft.App/managedEnvironments@2026-07-01' = {
  name: '${solutionName}-${environment}'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: reference(logAnalyticsWorkspaceId, '2023-09-01').customerId
        sharedKey: listKeys(logAnalyticsWorkspaceId, '2023-09-01').primarySharedKey
      }
    }
    workloadProfiles: [
      {
        workloadProfileType: 'Consumption'
        name: 'Consumption'
      }
    ]
    vnetConfiguration: {
      internal: false
      infrastructureSubnetId: virtualNetworkSubnetId
    }
    infrastructureResourceGroup: empty(infrastructureResourceGroup) ? null : infrastructureResourceGroup
    zoneRedundant: zoneRedundant
  }
}

resource fileShareRef 'Microsoft.App/managedEnvironments/storages@2026-07-01' = [for fileShareName in fileShareNames: {
  parent: appEnvironment
  name: fileShareName
  properties: {
    azureFile: {
      accountName: storageAccountName
      accountKey: storageAccountKey
      shareName: fileShareName
      accessMode: 'ReadWrite'
    }
  }
}]

output environmentId string = appEnvironment.id
output defaultDomain string = appEnvironment.properties.defaultDomain
