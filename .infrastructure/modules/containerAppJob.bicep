param location string
param solutionName string
param environment string

param jobName string

param acrName string
param image string

@description('Container registry login server. Defaults to the ACR derived from acrName.')
param registryServer string = ''

param environmentId string
param acrPullIdentityId string
param keyVaultAccessIdentityId string

param secrets array
param env array

@description('Optional command to override the container entrypoint. If empty, uses the container default.')
param command array = []

param resources object

param replicaTimeout int = 300
param replicaRetryLimit int = 1

@allowed([
  'Event'
  'Manual'
  'Schedule'
])
param triggerType string

param eventTriggerConfig object = {}
param manualTriggerConfig object = {
  parallelism: 1
  replicaCompletionCount: 1
}
param scheduleTriggerConfig object = {}

var registry = empty(registryServer) ? '${acrName}${az.environment().suffixes.acrLoginServer}' : registryServer

resource job 'Microsoft.App/jobs@2026-07-01' = {
  name: '${solutionName}-${jobName}-${environment}'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${acrPullIdentityId}': {}
      '${keyVaultAccessIdentityId}': {}
    }
  }
  properties: {
    environmentId: environmentId
    workloadProfileName: 'Consumption'
    configuration: {
      secrets: secrets
      triggerType: triggerType
      replicaTimeout: replicaTimeout
      replicaRetryLimit: replicaRetryLimit
      registries: [
        {
          server: registry
          identity: acrPullIdentityId
        }
      ]
      eventTriggerConfig: triggerType == 'Event' ? eventTriggerConfig : null
      manualTriggerConfig: triggerType == 'Manual' ? manualTriggerConfig : null
      scheduleTriggerConfig: triggerType == 'Schedule' ? scheduleTriggerConfig : null
    }
    template: {
      containers: [
        {
          image: '${registry}/${image}'
          name: '${solutionName}-${jobName}'
          env: env
          resources: resources
          command: empty(command) ? null : command
        }
      ]
    }
  }
}

output name string = job.name
output id string = job.id
