@export()
type customDomainType = {
  @description('Customer-managed sender domain, e.g. mail.example.com.')
  domain: string
  emailSenderUserName: string
  emailSenderDisplayName: string
}

param emailServiceName string
param customDomain customDomainType

resource emailService 'Microsoft.Communication/emailServices@2023-03-31' existing = {
  name: emailServiceName
}

resource customEmailDomain 'Microsoft.Communication/emailServices/domains@2023-03-31' = {
  parent: emailService
  name: customDomain.domain
  location: 'global'
  properties: {
    domainManagement: 'CustomerManaged'
  }
}

resource customSenderUserName 'Microsoft.Communication/emailServices/domains/senderUsernames@2023-03-31' = {
  parent: customEmailDomain
  name: customDomain.emailSenderUserName
  properties: {
    username: customDomain.emailSenderUserName
    displayName: customDomain.emailSenderDisplayName
  }
}

output domainId string = customEmailDomain.id
output mailFromAddress string = '${customSenderUserName.properties.username}@${customEmailDomain.properties.mailFromSenderDomain}'
output verificationRecords object = customEmailDomain.properties.verificationRecords
