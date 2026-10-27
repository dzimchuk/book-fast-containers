using 'bookfast.migrations.bicep'

param environment = 'dev'

param acrPullIdentity = {
  name: readEnvironmentVariable('BOOKFAST_ACR_PULL_IDENTITY_NAME', 'ACRPullIdentity')
  resourceGroup: readEnvironmentVariable('BOOKFAST_ACR_PULL_IDENTITY_RESOURCE_GROUP', 'TestContainerRegistry')
  subscriptionId: readEnvironmentVariable('BOOKFAST_ACR_PULL_IDENTITY_SUBSCRIPTION_ID')
}
