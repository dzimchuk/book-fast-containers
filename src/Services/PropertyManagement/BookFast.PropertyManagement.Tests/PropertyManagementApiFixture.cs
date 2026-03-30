using BookFast.Common.TestInfrastructure.IntegrationTest;
using Testcontainers.Azurite;

namespace BookFast.PropertyManagement.Tests
{
    public class PropertyManagementApiFixture : ApiFixture<Program>
    {
        private readonly AzuriteContainer azuriteContainer = new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:3.33.0")
            .WithInMemoryPersistence()
            .WithCommand("--skipApiVersionCheck")
            .Build();

        public override async Task InitializeAsync()
        {
            await azuriteContainer.StartAsync();

            Environment.SetEnvironmentVariable("BlobStorage:ConnectionString", azuriteContainer.GetConnectionString());

            await base.InitializeAsync();
        }

        public override async Task DisposeAsync()
        {
            await azuriteContainer.StopAsync();

            await base.DisposeAsync();
        }
    }
}
