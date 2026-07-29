using BookFast.Search.Store;
using MassTransit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;

namespace BookFast.Search.Tests.Search
{
    public class SearchFixture(SearchApiFixture apiFixture) : IAsyncLifetime
    {
        /// <summary>
        /// Test-only opt-in for running this suite against a real embeddings model.
        /// Set this env var (e.g. "AzureOpenAI") to opt a local test run
        /// into the real client; leave it unset to always get the deterministic fake.
        /// </summary>
        private const string TestEmbeddingsProviderEnvVar = "BOOKFAST_SEARCH_TESTS_EMBEDDINGS_PROVIDER";

        private WebApplicationFactory<Program> factory;

        public HttpClient HttpClient { get; private set; }

        public TestEmbeddingGenerator EmbeddingGenerator { get; private set; }

        public void RigEmbedding(string text, float[] vector) => EmbeddingGenerator.Rig(text, vector);

        public void RigEmbedding(Func<string, bool> matches, float[] vector) => EmbeddingGenerator.Rig(matches, vector);

        public int EmbedCallCount(Func<string, bool> matches) => EmbeddingGenerator.Calls.Count(matches);

        public Task InitializeAsync()
        {
            factory = apiFixture.GetWebApplicationFactory(services =>
            {
                services.PostConfigure<EmbeddingsOptions>(options =>
                    options.Provider = Environment.GetEnvironmentVariable(TestEmbeddingsProviderEnvVar) ?? string.Empty);

                var descriptor = services.Single(d => d.ServiceType == typeof(IEmbeddingGenerator<string, Embedding<float>>));
                services.Remove(descriptor);

                services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
                {
                    var options = sp.GetRequiredService<IOptions<EmbeddingsOptions>>().Value;
                    var realGenerator = string.IsNullOrEmpty(options.Provider)
                        ? null
                        : (IEmbeddingGenerator<string, Embedding<float>>)descriptor.ImplementationFactory(sp);

                    EmbeddingGenerator = new TestEmbeddingGenerator(realGenerator);

                    return EmbeddingGenerator;
                });
            });

            HttpClient = factory.CreateClient();

            return Task.CompletedTask;
        }

        public async Task DisposeAsync() => await factory.DisposeAsync();

        public async Task PublishAsync<TEvent>(TEvent message, CancellationToken cancellationToken = default)
            where TEvent : class
        {
            using var scope = factory.Services.CreateScope();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            await publishEndpoint.Publish(message, cancellationToken);
        }

        public async Task ResetAsync()
        {
            EmbeddingGenerator.Reset();

            using var scope = factory.Services.CreateScope();

            var collection = scope.ServiceProvider.GetRequiredService<VectorStoreCollection<Guid, AccommodationIndexRecord>>();
            await ClearAsync(collection, r => r.AccommodationId);

            var propertyCollection = scope.ServiceProvider.GetRequiredService<VectorStoreCollection<Guid, PropertyProjectionRecord>>();
            await ClearAsync(propertyCollection, r => r.PropertyId);
        }

        private static async Task ClearAsync<TRecord>(
            VectorStoreCollection<Guid, TRecord> collection,
            Func<TRecord, Guid> keySelector,
            CancellationToken cancellationToken = default)
            where TRecord : class
        {
            await Task.Delay(500, cancellationToken);

            var keys = await collection
                    .GetAsync(_ => true, top: 1000, cancellationToken: cancellationToken)
                    .Select(keySelector)
                    .ToListAsync(cancellationToken);

            if (keys.Count > 0)
            {
                await collection.DeleteAsync(keys, cancellationToken);
            }
        }

        internal async Task<TRecord> GetRecordAsync<TRecord>(Guid id, CancellationToken cancellationToken = default)
            where TRecord : class
        {
            using var scope = factory.Services.CreateScope();
            var collection = scope.ServiceProvider.GetRequiredService<VectorStoreCollection<Guid, TRecord>>();

            return await collection.GetAsync(id, cancellationToken: cancellationToken);
        }

        internal async Task WaitForPropertyRecordAsync(Guid propertyId, TimeSpan? timeout = null)
        {
            var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(10));

            while (true)
            {
                var record = await GetRecordAsync<PropertyProjectionRecord>(propertyId);
                if (record is not null)
                {
                    return;
                }

                if (DateTime.UtcNow >= deadline)
                {
                    throw new TimeoutException($"No property record for '{propertyId}' was found within the timeout.");
                }

                await Task.Delay(100);
            }
        }

        internal async Task<AccommodationIndexRecord> WaitForAccommodationRecordAsync(
            Guid accommodationId,
            Func<AccommodationIndexRecord, bool> predicate,
            TimeSpan? timeout = null)
        {
            var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(10));

            while (true)
            {
                var record = await GetRecordAsync<AccommodationIndexRecord>(accommodationId);
                if (predicate(record))
                {
                    return record;
                }

                if (DateTime.UtcNow >= deadline)
                {
                    throw new TimeoutException($"No accommodation record for '{accommodationId}' matching the predicate was found within the timeout.");
                }

                await Task.Delay(100);
            }
        }

        internal async Task PurgeAsync(DateTimeOffset olderThan, CancellationToken cancellationToken = default)
        {
            using var scope = factory.Services.CreateScope();
            var index = scope.ServiceProvider.GetRequiredService<AccommodationIndex>();

            await index.PurgeAsync(olderThan, cancellationToken);
        }
    }
}
