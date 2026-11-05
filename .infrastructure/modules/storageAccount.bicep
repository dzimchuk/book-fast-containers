param location string
param storageAccountName string

param containerNames array
param queueNames array
param fileShareNames array

param keyVaultName string
param connectionStringSecretName string
param keySecretName string = ''

param storageAccountSku string

param virtualNetworkSubnetId string

param allowPublicAccess bool = false

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: storageAccountSku
  }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    networkAcls: {
      resourceAccessRules: []
      bypass: 'AzureServices'
      virtualNetworkRules: virtualNetworkSubnetId != '' ? [
        {
          id: virtualNetworkSubnetId
          action: 'Allow'
          state: 'Succeeded'
        }
      ] : []
      ipRules: [
      ]
      defaultAction: allowPublicAccess ? 'Allow' : 'Deny'
    }
    supportsHttpsTrafficOnly: true
    encryption: {
      requireInfrastructureEncryption: false
      services: {
        file: {
          keyType: 'Account'
          enabled: true
        }
        blob: {
          keyType: 'Account'
          enabled: true
        }
      }
      keySource: 'Microsoft.Storage'
    }
    accessTier: 'Hot'
  }
}

resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
}

resource containers 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = [for containerName in containerNames: {
  parent: blobServices
  name: containerName
  properties: {
    immutableStorageWithVersioning: {
      enabled: false
    }
    publicAccess: 'None'
  }
}]

resource queueServices 'Microsoft.Storage/storageAccounts/queueServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
}

resource queues 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-05-01' = [for queueName in queueNames: {
  parent: queueServices
  name: queueName
}]

resource fileServices 'Microsoft.Storage/storageAccounts/fileServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    protocolSettings: {
      smb: {
        versions: 'SMB3.0;SMB3.1.1;'
        authenticationMethods: 'NTLMv2;Kerberos;'
        kerberosTicketEncryption: 'RC4-HMAC;AES-256;'
        channelEncryption: 'AES-128-CCM;AES-128-GCM;AES-256-GCM;'
      }
    }
    // cors: {
    //   corsRules: []
    // }
    shareDeleteRetentionPolicy: {
      enabled: true
      days: 7
    }
  }
}

resource fileShares 'Microsoft.Storage/storageAccounts/fileServices/shares@2023-05-01' = [for fileShareName in fileShareNames: {
  parent: fileServices
  name: fileShareName
  properties: {
    accessTier: 'TransactionOptimized'
    shareQuota: 5120
    enabledProtocols: 'SMB'
  }
}]

module storageConnection 'keyVaultSecret.bicep' = {
  name: '${storageAccountName}-connection'
  params: {
    keyVaultName: keyVaultName
    secretName: connectionStringSecretName
    secretValue: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${az.environment().suffixes.storage}'
  }
}

module storageAccountKey 'keyVaultSecret.bicep' = if (keySecretName != '') {
  name: '${storageAccountName}-key'
  params: {
    keyVaultName: keyVaultName
    secretName: keySecretName
    secretValue: storageAccount.listKeys().keys[0].value
  }
}

output name string = storageAccount.name
output id string = storageAccount.id
