using BookFast.Common.TestInfrastructure.IntegrationTest;

namespace BookFast.Identity.Tests
{
    [CollectionDefinition(nameof(IntegrationTestCollection))]
    public sealed class IntegrationTestCollection : ICollectionFixture<ApiFixture<Program>>;
}
