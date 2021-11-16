# Book Fast (Docker)
A sample demonstrating how to implement a containerized multitenant facility management and accommodation booking application. It uses microservices architecture and relies on a bunch of Azure services.

## Running locally in Docker Compose

```
docker-compose  -f "docker-compose.yml" -f "docker-compose.development.yml" --no-ansi build
docker-compose  -f "docker-compose.yml" -f "docker-compose.development.yml" --no-ansi up -d --no-build --force-recreate --remove-orphans
```

## Features

### Architecture
- 4 bounded contexts
- CQRS and DDD (with [reliable domain events](https://dzimchuk.net/reliable-domain-events/))
- ASP.NET Core 6, Blazor, gRPC
- Kubernetes and Docker Compose

### Build and deployment
- GitHub actions
- Azure Container Registry
- Helm

### Security
- [Multitenant](https://dzimchuk.net/enabling-multitenant-support-in-you-azure-ad-protected-applications/) Azure AD organizational accounts
- [Azure AD B2C](https://dzimchuk.net/setting-up-your-asp-net-core-2-0-apps-and-services-for-azure-ad-b2c/) authentication for customers
- OpenID Connect and OAuth2

### Azure services
- Azure Kubernetes Service
- Azure Container Registry
- Azure SQL databases
- Azure Storage
- Azure Service Bus
- Azure Search
- Application Insights
- Azure KeyVault

## Configuration

The application supports Development, Staging and Production environments. In Development it relies on [User Secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets) and in Staging/Production it uses Azure KeyVault.

Here's a short description of configuration parameters:

```
{
  "ApplicationInsights": {
    "InstrumentationKey": "Application Insights resource instrumentation key"
  },
  "Authentication": {
    "AzureAd": {
      "Instance": "Your Azure AD instance, e.g. https://login.microsoftonline.com/",
      "Audience": "BookFast API AppId in Azure AD, e.g. https://devunleashed.onmicrosoft.com/book-fast-api",
      "ValidIssuers": "Comma separated list of tenant identifiers, e.g. https://sts.windows.net/490789ec-b183-4ba5-97cf-e69ec8870130/,https://sts.windows.net/f418e7eb-0dcd-40be-9b81-c58c87c57d9a/"
    }
  },
  "ServiceBus": {
    "ConnectionString": "Connection string to Service Bus topic",
    "Facility": {
      "NotificationQueueConnection": "Connection string to the Facility service notification queue"
    },
    "Booking": {
      "NotificationQueueConnection": "Connection string to the Booking service notification queue"
    }
  },
  "Data": {
    "Azure": {
      "Storage": {
        "ConnectionString": "Connection string to an Azure storage account"
      }
    },
    "DefaultConnection": {
      "ConnectionString": "Connection string to a SQL database"
    }
  },
  "Search": {
    "QueryKey": "Azure Search query key",
    "AdminKey": "Azure Search admin key",
    "ServiceEndpoint": "Azure Search service endpoint",
    "IndexName": "Azure Search index"
  }
}
```

### User Secrets

All services are configured to use the same User Secrets ID for simplicity. So it's enough to configure User Secrets with the settings shown above once to be used in Development environment.

### Azure KeyVault

#### Kubernetes

The application has been designed to run in Azure Kubernetes Service (AKS). In Staging and Production modes services are configured to use pod identity called `my-pod-identity` (see deployment files). [This article](https://docs.microsoft.com/en-us/azure/aks/use-azure-ad-pod-identity) provides details on how to set up pod identity in the application namespace. 

The pod identity in question is expected to reference a user-assigned managed identity that is configured to access KeyVault (list and get permissions).

#### Docker Compose

Currently Docker Compose is supposed to be used locally on a dev box. In Production mode (that is used for demo purpose only) the following directory gets mounted to containers: `${OneDrive}/dev/BookFast/KeyVault`

The directory is expected to contain 4 plain text files:

- tenantId
- clientId
- clientSecret
- keyVaultName

File names are pretty much self explonatory. Just make sure they don't contain extra spaces or line feeds. The app is going to try to create a `ClientSecretCredential` to authenticate to KeyVault.

Alternatively it can also try to use a `DefaultAzureCredential` if there is a `KeyVaultName` environment variable. But in this case make sure to also define the following variables for `DefaultAzureCredential` to work:

- AZURE_CLIENT_ID
- AZURE_CLIENT_SECRET
- AZURE_TENANT_ID

In both cases you need to have a corresponding principal in your Azure AD tenant that is also configured to access KeyVault secrets (list and get permissions).


### Azure AD

Azure AD is used for organizational accounts of facility providers. You will need two applications in Azure AD: one for the APIs (Book Fast API app) and one for the web (BookFast app). Both applications should have multitenant support enabled. BookFast should have a delegated permission to access BookFast API app. If you're new to Azure AD the following post are going to help you out:

- [Protecting your APIs with Azure Active Directory](https://dzimchuk.net/protecting-your-apis-with-azure-active-directory/)
- [Enabling multitenant support in you Azure AD protected applications](https://dzimchuk.net/enabling-multitenant-support-in-you-azure-ad-protected-applications/)

Both apps have a user role called 'Facility Provider' that should be assigned to users to enable them to edit facilities. Please have a look at this [post](https://dzimchuk.net/application-and-user-permissions-in-azure-ad/) to understand how application and user roles are configured in Azure AD.

### Azure AD B2C

Customer accounts are managed in Azure AD B2C. It supports self sign up, profile editing and 3rd part identity providers.

You will need to create a B2C tenant and an app. You will also need to policies:

1. Sign in or sign up policy
2. Profile edit policy

You may also find this [post](https://dzimchuk.net/setting-up-your-asp-net-core-2-0-apps-and-services-for-azure-ad-b2c/) useful when setting you your application.

### SQL Database

`BookFast.Facility.Data` and `BookFast.Booking.Data` projects contain EFCore migrations to set up you SQL database schema.

### Service Bus
Azure Service Bus is used as a message broker for integration events.

Please make sure to provision a single topic with 3 subscriptions:
- Booking
- Facility
- SearchIndexer

Also provision 2 notification queues:
- bookfast-facility-notifications
- bookfast-booking-notifications

### Azure Search

BookFast.Search.Adapter can be run from the command line as `dotnet run provision` in order to create an index in your Azure Search service. It will require the following parameters to be defined in [user secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets):

- Search:ServiceEndpoint
- Search:AdminKey
- Search:IndexName