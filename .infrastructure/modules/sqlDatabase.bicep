param location string

@description('Region for the SQL server and database. Defaults to the main location; override when the region is not accepting new SQL servers. The private endpoint stays in the main location, next to the virtual network.')
param sqlServerLocation string = location

param solutionName string
param environment string

@secure()
@description('Administrator login username for the SQL server.')
param sqlServerAdministratorLogin string

@secure()
@description('Administrator login password for the SQL server.')
param sqlServerAdministratorPassword string

@description('SQL database SKU and other parameters.')
param sqlDatabaseParameters object

param virtualNetworkId string
param virtualNetworkServicesSubnetId string

param keyVaultName string
param connectionStringSecretName string

param publicNetworkAccess bool

@description('Public IPv4 addresses allowed through the SQL server firewall (e.g. to be used by the deploy pipeline to apply migrations).')
param sqlServerAllowedIpAddresses array = []

var name = sqlDatabaseParameters.prefix != '' ? '${sqlDatabaseParameters.prefix}-${solutionName}-${environment}' : '${solutionName}-${environment}'

resource sqlServer 'Microsoft.Sql/servers@2025-02-01-preview' = {
  name: name
  location: sqlServerLocation
  properties: {
    administratorLogin: sqlServerAdministratorLogin
    administratorLoginPassword: sqlServerAdministratorPassword
    // ipRules only take effect when public network access is Enabled, so a non-empty allowlist
    // forces Enabled (mirrors keyVault.bicep's publicNetworkAccess handling)
    publicNetworkAccess: (publicNetworkAccess || !empty(sqlServerAllowedIpAddresses)) ? 'Enabled' : 'Disabled'
  }
}

resource sqlFirewallRules 'Microsoft.Sql/servers/firewallRules@2025-02-01-preview' = [for ip in sqlServerAllowedIpAddresses: {
  name: 'Allow-${ip}'
  parent: sqlServer
  properties: {
    startIpAddress: ip
    endIpAddress: ip
  }
}]

resource sqlDatabase 'Microsoft.Sql/servers/databases@2025-02-01-preview' = {
  parent: sqlServer
  name: name
  location: sqlServerLocation
  sku: sqlDatabaseParameters.sku
  properties: {
    minCapacity: json('${sqlDatabaseParameters.minCapacity}')
  }
}

var privateEndpointName = 'sql-private-endpoint'
resource privateEndpoint 'Microsoft.Network/privateEndpoints@2025-07-01' = {
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
          privateLinkServiceId: sqlServer.id
          groupIds: [
            'SqlServer'
          ]
        }
      }
    ]
  }
}

var dnsZoneName = 'privatelink${az.environment().suffixes.sqlServerHostname}'
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

resource privateEndpointDnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2025-07-01' = {
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

module storageConnection 'keyVaultSecret.bicep' = {
  name: '${sqlDatabase.name}-connection'
  params: {
    keyVaultName: keyVaultName
    secretName: connectionStringSecretName
    secretValue: 'Server=tcp:${sqlServer.name}${az.environment().suffixes.sqlServerHostname},1433;Initial Catalog=${sqlDatabase.name};Persist Security Info=False;User ID=${sqlServerAdministratorLogin};Password=${sqlServerAdministratorPassword};MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
  }
}

output sqlDatabaseName string = sqlDatabase.name
output sqlServerName string = sqlServer.name
