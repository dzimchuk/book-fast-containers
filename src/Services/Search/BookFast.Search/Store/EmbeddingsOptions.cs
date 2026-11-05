namespace BookFast.Search.Store
{
    internal class EmbeddingsOptions
    {
        public const int VectorDimensionSize = 768;

        public string Provider { get; set; }

        public OllamaEmbeddingsOptions Ollama { get; set; } = new();

        public AzureOpenAIEmbeddingsOptions AzureOpenAI { get; set; } = new();
    }

    internal class OllamaEmbeddingsOptions
    {
        public string Endpoint { get; set; }

        public string Model { get; set; } = "nomic-embed-text";
    }

    internal class AzureOpenAIEmbeddingsOptions
    {
        public string Endpoint { get; set; }

        public string ApiKey { get; set; }

        public string Model { get; set; } = "text-embedding-3-small";
    }
}
