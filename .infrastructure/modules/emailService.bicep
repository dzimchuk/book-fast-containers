param solutionName string
param environment string

@description('Name of the Communication Service resource.')
param commServiceName string = '${solutionName}-${environment}'

param keyVaultName string

@description('Name of the Key Vault secret holding the Communication Service connection string.')
param connectionStringSecretName string = 'communication-service-connection-string'

import { customDomainType } from 'emailCustomDomain.bicep'

@description('Optional customer-managed sender domain. Leave null to use only the Azure-managed domain. Its DNS records must be verified before the domain is usable for sending.')
param customDomain customDomainType?

var emailServiceName = '${solutionName}-${environment}'

resource emailService 'Microsoft.Communication/emailServices@2023-03-31' = {
  name: emailServiceName
  location: 'global'
  properties: {
    dataLocation: 'Europe'
  }
}

resource emailServiceDomain 'Microsoft.Communication/emailServices/domains@2023-03-31' = {
  parent: emailService
  name: 'AzureManagedDomain'
  location: 'global'
  properties: {
    domainManagement: 'AzureManaged'
  }
}

resource senderUserName 'Microsoft.Communication/emailServices/domains/senderUsernames@2023-03-31' = {
  parent: emailServiceDomain
  name: 'donotreply'
  properties: {
    username: 'DoNotReply'
    displayName: 'DoNotReply'
  }
}

module customEmailDomain 'emailCustomDomain.bicep' = if (customDomain != null) {
  name: '${solutionName}-${environment}-email-custom-domain'
  params: {
    emailServiceName: emailService.name
    customDomain: customDomain!
  }
}

resource communicationService 'Microsoft.Communication/communicationServices@2023-03-31' = {
  name: commServiceName
  location: 'global'
  properties: {
    dataLocation: 'Europe'
    linkedDomains: concat([
      emailServiceDomain.id
    ], customDomain != null ? [
      customEmailDomain!.outputs.domainId
    ] : [])
  }
}

module communicationServiceConnection 'keyVaultSecret.bicep' = {
  name: '${solutionName}-commService-connection-${environment}'
  params: {
    keyVaultName: keyVaultName
    secretName: connectionStringSecretName
    secretValue: communicationService.listKeys().primaryConnectionString
  }
}

output communicationServiceId string = communicationService.id
output connectionStringSecretName string = connectionStringSecretName
output mailFromAddress string = '${senderUserName.properties.username}@${emailServiceDomain.properties.mailFromSenderDomain}'
output customDomainMailFromAddress string = customDomain != null ? customEmailDomain!.outputs.mailFromAddress : ''
output customDomainVerificationRecords object = customDomain != null ? customEmailDomain!.outputs.verificationRecords : {}
