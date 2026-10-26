using 'bookfast.apps.bicep'

param environment = 'dev'

param acrPullIdentity = {
  name: readEnvironmentVariable('BOOKFAST_ACR_PULL_IDENTITY_NAME', 'ACRPullIdentity')
  resourceGroup: readEnvironmentVariable('BOOKFAST_ACR_PULL_IDENTITY_RESOURCE_GROUP', 'TestContainerRegistry')
  subscriptionId: readEnvironmentVariable('BOOKFAST_ACR_PULL_IDENTITY_SUBSCRIPTION_ID')
}

var defaultResources = {
  cpu: '1.0'
  memory: '2Gi'
}

var defaultScale = {
  minReplicas: 1
  maxReplicas: 2
  cooldownPeriod: 300
  pollingInterval: 30
  rules: [
    {
      name: 'http-scaler'
      http: {
        metadata: {
          concurrentRequests: '100'
        }
      }
    }
    {
      name: 'cpu-scaler'
      custom: {
        type: 'cpu'
        metadata: {
          type: 'Utilization'
          value: '70'
        }
      }
    }
    {
      name: 'memory-scaler'
      custom: {
        type: 'memory'
        metadata: {
          type: 'Utilization'
          value: '85'
        }
      }
    }
  ]
}

param containerAppsConfig = {
  aspNetCoreEnvironment: 'Production'

  identity: {
    resources: defaultResources
    scale: defaultScale
    customDomainName: ''
    mailFromAddress: ''
  }

  propertyManagement: {
    resources: defaultResources
    scale: defaultScale
    customDomainName: ''
    blobContainerName: 'property-images'
  }

  search: {
    resources: defaultResources
    scale: defaultScale
    customDomainName: ''
    embeddingsModel: 'text-embedding-3-small'
    purge: {
      window: '1.00:00:00'
      interval: '00:15:00'
    }
  }

  booking: {
    resources: defaultResources
    scale: defaultScale
    customDomainName: ''
    payments: {
      settlementDelay: '00:00:05'
      sweepInterval: '00:00:02'
    }
    reservationExpiration: {
      sweepInterval: '00:00:02'
    }
  }
}
