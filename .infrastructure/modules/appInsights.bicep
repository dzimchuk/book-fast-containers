param location string

@minLength(3)
param solutionName string

param environment string

param keyVaultName string
param connectionStringSecretName string

resource workspace 'Microsoft.OperationalInsights/workspaces@2025-02-01' = {
  name: '${solutionName}-${environment}'
  location: location
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${solutionName}-${environment}'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: workspace.id
  }
}

module connectionStringSecret 'keyVaultSecret.bicep' = {
  name: '${solutionName}-${environment}-appinsights-connection'
  params: {
    keyVaultName: keyVaultName
    secretName: connectionStringSecretName
    secretValue: appInsights.properties.ConnectionString
  }
}

output appInsightsResourceId string = appInsights.id
output logAnalyticsResourceId string = workspace.id
