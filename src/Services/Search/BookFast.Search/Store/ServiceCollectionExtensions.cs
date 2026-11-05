using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents.Indexes;
using CommunityToolkit.VectorData.AzureAISearch;
using CommunityToolkit.VectorData.Qdrant;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using OllamaSharp;
using Qdrant.Client;

namespace BookFast.Search.Store
{
    internal static class ServiceCollectionExtensions
    {
        private const string CollectionName = "accommodations";
        private const string PropertyCollectionName = "properties";

        public static IServiceCollection AddSearchStore(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<StoreOptions>(configuration.GetSection("Store"));

            services.AddSingleton<VectorStore>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<StoreOptions>>().Value;

                if (string.Equals(options.Provider, "AzureAISearch", StringComparison.OrdinalIgnoreCase))
                {
                    var endpoint = options.AzureAISearch.Endpoint
                        ?? throw new InvalidOperationException("Store:AzureAISearch:Endpoint not configured.");
                    var apiKey = options.AzureAISearch.ApiKey
                        ?? throw new InvalidOperationException("Store:AzureAISearch:ApiKey not configured.");

                    var searchIndexClient = new SearchIndexClient(new Uri(endpoint), new AzureKeyCredential(apiKey));

                    return new AzureAISearchVectorStore(searchIndexClient);
                }

                var connectionString = options.Qdrant.ConnectionString
                    ?? throw new InvalidOperationException("Store:Qdrant:ConnectionString not configured.");

                var qdrantClient = new QdrantClient(new Uri(connectionString));

                return new QdrantVectorStore(qdrantClient, ownsClient: true);
            });

            services.AddSingleton<VectorStoreCollection<Guid, AccommodationIndexRecord>>(sp => sp.GetRequiredService<VectorStore>()
                .GetCollection<Guid, AccommodationIndexRecord>(CollectionName));

            services.AddSingleton<VectorStoreCollection<Guid, PropertyProjectionRecord>>(sp => sp.GetRequiredService<VectorStore>()
                .GetCollection<Guid, PropertyProjectionRecord>(PropertyCollectionName));

            services.Configure<EmbeddingsOptions>(configuration.GetSection("Embeddings"));

            services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<EmbeddingsOptions>>().Value;

                if (string.Equals(options.Provider, "AzureOpenAI", StringComparison.OrdinalIgnoreCase))
                {
                    var endpoint = options.AzureOpenAI.Endpoint
                        ?? throw new InvalidOperationException("Store:AzureOpenAI:Endpoint not configured.");
                    var apiKey = options.AzureOpenAI.ApiKey
                        ?? throw new InvalidOperationException("Store:AzureOpenAI:ApiKey not configured.");

                    var azureClient = new AzureOpenAIClient(new Uri(options.AzureOpenAI.Endpoint), new System.ClientModel.ApiKeyCredential(options.AzureOpenAI.ApiKey));
                    return azureClient.GetEmbeddingClient(deploymentName: options.AzureOpenAI.Model)
                        .AsIEmbeddingGenerator();
                }
                else if (string.Equals(options.Provider, "Ollama", StringComparison.OrdinalIgnoreCase))
                {
                    var endpoint = options.Ollama.Endpoint
                        ?? throw new InvalidOperationException("Embeddings:Ollama:Endpoint not configured.");

                    return new OllamaApiClient(new Uri(endpoint), options.Ollama.Model);
                }

                throw new InvalidOperationException("Unsupported embeddings provider.");
            });

            services.AddScoped<AccommodationIndex>();
            services.AddScoped<PropertyProjection>();

            services.Configure<PurgeOptions>(configuration.GetSection("Purge"));
            services.AddHostedService<PurgeService>();

            return services;
        }

        public static async Task EnsureSearchStoreCreatedAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var accommodationCollection = scope.ServiceProvider.GetRequiredService<VectorStoreCollection<Guid, AccommodationIndexRecord>>();
            await accommodationCollection.EnsureCollectionExistsAsync();

            var propertyCollection = scope.ServiceProvider.GetRequiredService<VectorStoreCollection<Guid, PropertyProjectionRecord>>();
            await propertyCollection.EnsureCollectionExistsAsync();
        }
    }
}
