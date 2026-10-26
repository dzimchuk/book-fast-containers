param keyVaultName string
param secretName string

@secure()
param secretValue string

param contentType string = ''

resource secret 'Microsoft.KeyVault/vaults/secrets@2024-11-01' = {
  name: '${keyVaultName}/${secretName}'
  properties: {
    value: secretValue
    contentType: contentType
  }
}
