using BookFast.Common.TestInfrastructure.IntegrationTest;
using BookFast.PropertyManagement.Integration;
using Testcontainers.Azurite;

namespace BookFast.PropertyManagement.Tests
{
    public class PropertyManagementApiFixture : ApiFixture<Program>
    {
        private readonly AzuriteContainer azuriteContainer = new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:3.33.0")
            .WithInMemoryPersistence()
            .WithCommand("--skipApiVersionCheck")
            .Build();

        public IntegrationEventHarness IntegrationEvents { get; } = new IntegrationEventHarness()
            .Observe<AccommodationCreatedEvent>()
            .Observe<AccommodationUpdatedEvent>()
            .Observe<AccommodationDeletedEvent>();

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            await azuriteContainer.StartAsync();

            Environment.SetEnvironmentVariable("BlobStorage:ConnectionString", azuriteContainer.GetConnectionString());

            Environment.SetEnvironmentVariable("Outbox:QueryDelay", "00:00:01");     
            await IntegrationEvents.StartAsync();
        }

        public override async Task DisposeAsync()
        {
            await IntegrationEvents.StopAsync();

            await azuriteContainer.StopAsync();

            await base.DisposeAsync();
        }
    }
}

