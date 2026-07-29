using BookFast.Common.TestInfrastructure.IntegrationTest;
using Testcontainers.Qdrant;

namespace BookFast.Search.Tests
{
    public class SearchApiFixture : ApiFixture<Program>
    {
        private readonly QdrantContainer qdrantContainer = new QdrantBuilder("qdrant/qdrant:latest").Build();

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            await qdrantContainer.StartAsync();

            Environment.SetEnvironmentVariable("Store:Qdrant:ConnectionString", qdrantContainer.GetGrpcConnectionString());
        }

        public override async Task DisposeAsync()
        {
            await qdrantContainer.StopAsync();

            await base.DisposeAsync();
        }
    }
}

