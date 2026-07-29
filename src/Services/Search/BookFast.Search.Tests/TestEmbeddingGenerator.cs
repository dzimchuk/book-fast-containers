using BookFast.Search.Store;
using Microsoft.Extensions.AI;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace BookFast.Search.Tests
{
    /// <summary>
    /// Deterministic fake (same text -> same vector). Also supports "rigging" - forcing an explicit vector
    /// for text matching a predicate - so a test can make an unrelated query phrase resolve to the same vector
    /// as a target document's composed text, exercising the vector arm's plumbing without a real embedding model.
    /// Tracks every call so tests can assert whether re-embedding happened.
    ///
    /// Optionally wraps a real <see cref="IEmbeddingGenerator{String, Embedding}"/> (e.g. Azure OpenAI/Ollama):
    /// when supplied, any text that isn't rigged is delegated to it instead of the deterministic hash, so the
    /// same test suite can run end-to-end against a real embedding model while rigging and call-tracking still
    /// work exactly as they do against the pure fake.
    /// </summary>
    public class TestEmbeddingGenerator(IEmbeddingGenerator<string, Embedding<float>> inner = null) : IEmbeddingGenerator<string, Embedding<float>>
    {
        private readonly List<(Func<string, bool> Matches, float[] Vector)> rigs = [];

        /// <summary>Every text passed to <see cref="GenerateAsync"/>, for test assertions (e.g. "was this
        /// record re-embedded?").</summary>
        public ConcurrentBag<string> Calls { get; } = [];

        /// <summary>Forces the given exact text to embed to <paramref name="vector"/>.</summary>
        public void Rig(string text, float[] vector) => Rig(t => t == text, vector);

        /// <summary>Forces any text matching <paramref name="matches"/> to embed to <paramref name="vector"/>.</summary>
        public void Rig(Func<string, bool> matches, float[] vector) => rigs.Add((matches, vector));

        /// <summary>Clears all rigging and the call log. Called between tests.</summary>
        public void Reset()
        {
            rigs.Clear();
            Calls.Clear();
        }

        public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
            IEnumerable<string> values,
            EmbeddingGenerationOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var results = new List<Embedding<float>>();

            foreach (var value in values)
            {
                var text = value ?? string.Empty;
                Calls.Add(text);

                results.Add(new Embedding<float>(await ResolveAsync(text, options, cancellationToken)));
            }

            return new GeneratedEmbeddings<Embedding<float>>(results);
        }

        public object GetService(Type serviceType, object serviceKey = null) => null;

        public void Dispose() => inner?.Dispose();

        private async Task<float[]> ResolveAsync(string text, EmbeddingGenerationOptions options, CancellationToken cancellationToken)
        {
            foreach (var (matches, vector) in rigs)
            {
                if (matches(text))
                {
                    return vector;
                }
            }

            if (inner is not null)
            {
                var embeddings = await inner.GenerateAsync([text], options, cancellationToken);
                return embeddings[0].Vector.ToArray();
            }

            return ToVector(text);
        }

        private static float[] ToVector(string text)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text ?? string.Empty));

            // Cycle the 32-byte hash to fill the vector deterministically
            var vector = new float[EmbeddingsOptions.VectorDimensionSize];
            for (var i = 0; i < vector.Length; i++)
            {
                vector[i] = hash[i % hash.Length] / 255f;
            }

            return vector;
        }
    }
}
