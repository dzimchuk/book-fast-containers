namespace BookFast.PropertyManagement.Tests
{
    [CollectionDefinition(nameof(IntegrationTestCollection))]
    public sealed class IntegrationTestCollection : ICollectionFixture<PropertyManagementApiFixture>;
}
