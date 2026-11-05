using 'bookfast.migrations.bicep'

param environment = 'dev'

// GitHub Actions exports unset variables as empty strings, which readEnvironmentVariable does not treat as unset
var acrPullIdentityName = readEnvironmentVariable('BOOKFAST_ACR_PULL_IDENTITY_NAME', 'ACRPullIdentity')
var acrPullIdentityResourceGroup = readEnvironmentVariable('BOOKFAST_ACR_PULL_IDENTITY_RESOURCE_GROUP', 'TestContainerRegistry')

param acrPullIdentity = {
  name: empty(acrPullIdentityName) ? 'ACRPullIdentity' : acrPullIdentityName
  resourceGroup: empty(acrPullIdentityResourceGroup) ? 'TestContainerRegistry' : acrPullIdentityResourceGroup
  subscriptionId: readEnvironmentVariable('BOOKFAST_ACR_PULL_IDENTITY_SUBSCRIPTION_ID')
}
