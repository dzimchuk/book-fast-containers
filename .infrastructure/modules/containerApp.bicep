param location string
param solutionName string
param environment string

param appName string

param acrName string
param image string

@description('Container registry login server. Defaults to the ACR derived from acrName.')
param registryServer string = ''

param environmentId string
param acrPullIdentityId string
param keyVaultAccessIdentityId string

param targetPort int = 0
param customDomainName string = ''
param createManagedCert bool = false

param secrets array
param env array

param resources object
param scale object

param probes array = []

@description('Optional command to override the container entrypoint. If empty, uses the container default.')
param command array = []

@description('Set to true to deploy as a Function App, false for a regular Container App.')
param isFunctionApp bool = false

var registry = empty(registryServer) ? '${acrName}${az.environment().suffixes.acrLoginServer}' : registryServer

resource containerApp 'Microsoft.App/containerApps@2026-07-01' = {
  name: '${solutionName}-${appName}-${environment}'
  location: location
  kind: isFunctionApp ? 'functionapp' : null
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
      activeRevisionsMode: 'Single'
      ingress: targetPort == 0 ? null : {
        external: true
        targetPort: targetPort
        transport: 'Auto'
        customDomains: empty(customDomainName) ? [] : [
          {
            name: customDomainName
            bindingType: 'Auto'
          }
        ]
        traffic: [
          {
            weight: 100
            latestRevision: true
          }
        ]
        allowInsecure: false
        stickySessions: {
          affinity: 'none'
        }
      }
      registries: [
        {
           server: registry
           identity: acrPullIdentityId
        }
      ]
    }
    template: {
      containers: [
        {
          image: '${registry}/${image}'
          name: '${solutionName}-${appName}'
          env: env
          resources: resources
          probes: probes
          command: empty(command) ? null : command
        }
      ]
      scale: scale
    }
  }
}

resource appEnvironment 'Microsoft.App/managedEnvironments@2026-07-01' existing = {
  name: '${solutionName}-${environment}'
}

resource managedCertificate 'Microsoft.App/managedEnvironments/managedCertificates@2026-07-01' = if (createManagedCert) {
  parent: appEnvironment
  name: customDomainName
  location: location
  properties: {
    subjectName: customDomainName
    domainControlValidation: 'CNAME' // 'HTTP' for apex domains
  }
  dependsOn: [
    containerApp
  ]
}

output url string = containerApp.properties.configuration.ingress != null ? containerApp.properties.configuration.ingress.fqdn : ''
