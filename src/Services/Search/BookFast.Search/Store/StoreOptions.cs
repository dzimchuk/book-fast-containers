namespace BookFast.Search.Store
{
    internal class StoreOptions
    {
        public string Provider { get; set; }

        public QdrantStoreOptions Qdrant { get; set; } = new();

        public AzureAISearchStoreOptions AzureAISearch { get; set; } = new();
    }

    internal class QdrantStoreOptions
    {
        public string ConnectionString { get; set; }
    }

    internal class AzureAISearchStoreOptions
    {
        public string Endpoint { get; set; }

        public string ApiKey { get; set; }
    }
}
