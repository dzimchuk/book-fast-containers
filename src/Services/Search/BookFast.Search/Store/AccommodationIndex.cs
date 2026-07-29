using BookFast.Common.Application.Queries;
using BookFast.Common.Domain;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace BookFast.Search.Store
{
    internal class AccommodationIndex(
        VectorStoreCollection<Guid, AccommodationIndexRecord> collection,
        VectorStoreCollection<Guid, PropertyProjectionRecord> propertyCollection,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
    {
        public async Task UpsertAsync(AccommodationIndexRecord record, CancellationToken cancellationToken = default)
        {
            var existing = await collection.GetAsync(
                record.AccommodationId,
                new RecordRetrievalOptions { IncludeVectors = true },
                cancellationToken);

            if (existing is not null && existing.AccommodationOccurredAt >= record.AccommodationOccurredAt)
            {
                return;
            }

            var property = await propertyCollection.GetAsync(Guid.Parse(record.PropertyId), cancellationToken: cancellationToken);

            Denormalize(record, property, existing);

            record.Active = true;

            await ApplySearchTextAndEmbeddingAsync(record, existing, cancellationToken);

            await collection.UpsertAsync(record, cancellationToken);
        }

        public async Task DeleteAsync(Guid accommodationId, string tenantId, Guid propertyId, DateTimeOffset occurredAt, CancellationToken cancellationToken = default)
        {
            // include the vector: it must round-trip unchanged into the re-upsert below, otherwise it would
            // be overwritten with an empty one
            var existing = await collection.GetAsync(
                accommodationId,
                new RecordRetrievalOptions { IncludeVectors = true },
                cancellationToken);

            if (existing is not null && existing.AccommodationOccurredAt >= occurredAt)
            {
                return;
            }

            // A never-seen-before delete (a race with its Created/Updated) still needs a minimal record
            var record = existing ?? new AccommodationIndexRecord
            {
                AccommodationId = accommodationId,
                TenantId = tenantId,
                PropertyId = propertyId.ToString(),
                Name = string.Empty,
                SearchText = string.Empty,
                ContentHash = ComputeHash(string.Empty),
                Embedding = await GenerateEmbeddingAsync(string.Empty, cancellationToken),
            };

            record.Active = false;
            record.AccommodationOccurredAt = occurredAt;

            await collection.UpsertAsync(record, cancellationToken);
        }

        public async Task ReindexPropertyAsync(Guid propertyId, CancellationToken cancellationToken = default)
        {
            var property = await propertyCollection.GetAsync(propertyId, cancellationToken: cancellationToken);

            if (property is null)
            {
                return;
            }

            var propertyIdText = propertyId.ToString();

            var accommodations = await collection
                .GetAsync(
                    r => r.PropertyId == propertyIdText,
                    top: 1000,
                    new FilteredRecordRetrievalOptions<AccommodationIndexRecord> { IncludeVectors = true },
                    cancellationToken)
                .ToListAsync(cancellationToken);

            foreach (var accommodation in accommodations)
            {
                if (accommodation.PropertyOccurredAt is { } propertyOccurredAt && propertyOccurredAt >= property.OccurredAt)
                {
                    continue;
                }

                Denormalize(accommodation, property, accommodation);

                await ApplySearchTextAndEmbeddingAsync(accommodation, accommodation, cancellationToken);

                await collection.UpsertAsync(accommodation, cancellationToken);
            }
        }

        public async Task PurgeAsync(DateTimeOffset olderThan, CancellationToken cancellationToken = default)
        {
            var stale = await collection
                .GetAsync(r => !r.Active && r.AccommodationOccurredAt < olderThan, top: 1000, cancellationToken: cancellationToken)
                .Select(r => r.AccommodationId)
                .ToListAsync(cancellationToken);

            if (stale.Count > 0)
            {
                await collection.DeleteAsync(stale, cancellationToken);
            }
        }

        public async Task<ListQueryResult<SearchResult>> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize < 1 ? 20 : Math.Min(query.PageSize, 50);
            var toSkip = (pageNumber - 1) * pageSize;

            var filter = BuildFilter(query);

            List<AccommodationIndexRecord> pageEntities;

            if (!string.IsNullOrWhiteSpace(query.Query))
            {
                var keywords = query.Query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var vector = await GenerateEmbeddingAsync(query.Query, cancellationToken);

                var hybridSearchable = (IKeywordHybridSearchable<AccommodationIndexRecord>)collection;

                pageEntities = await hybridSearchable.HybridSearchAsync(
                        vector,
                        keywords,
                        top: pageSize,
                        new HybridSearchOptions<AccommodationIndexRecord>
                        {
                            Filter = filter,
                            AdditionalProperty = r => r.SearchText,
                            Skip = toSkip,
                        },
                        cancellationToken)
                    .Select(r => r.Record)
                    .ToListAsync(cancellationToken);
            }
            else
            {
                pageEntities = await collection
                    .GetAsync(filter, top: pageSize, new FilteredRecordRetrievalOptions<AccommodationIndexRecord> { Skip = toSkip }, cancellationToken)
                    .ToListAsync(cancellationToken);
            }

            var pageRecords = pageEntities.Select(ToSearchResult);

            return new ListQueryResult<SearchResult>
            {
                Records = pageRecords,
                PageNumber = pageNumber,
                TotalRecords = null,
                TotalPages = null,
            };
        }

        private static Expression<Func<AccommodationIndexRecord, bool>> BuildFilter(SearchQuery query)
        {
            Expression<Func<AccommodationIndexRecord, bool>> filter = r => r.Active;

            if (query.Bedrooms.HasValue)
            {
                int? bedrooms = query.Bedrooms.Value;
                filter = filter.AndAlso(r => r.Bedrooms == bedrooms);
            }

            foreach (var facility in query.Facilities ?? [])
            {
                filter = filter.AndAlso(r => r.Facilities.Contains(facility));
            }

            if (!string.IsNullOrWhiteSpace(query.City))
            {
                var city = query.City;
                filter = filter.AndAlso(r => r.City == city);
            }

            if (!string.IsNullOrWhiteSpace(query.Country))
            {
                var country = query.Country;
                filter = filter.AndAlso(r => r.Country == country);
            }

            return filter;
        }

        private static void Denormalize(AccommodationIndexRecord record, PropertyProjectionRecord property, AccommodationIndexRecord fallback)
        {
            if (property is not null)
            {
                record.PropertyName = property.Name;
                record.PropertyDescription = property.Description;
                record.City = property.City;
                record.Country = property.Country;
                record.Latitude = property.Latitude;
                record.Longitude = property.Longitude;
                record.Facilities = record.OwnFacilities.Union(property.Facilities ?? []).ToArray();
                record.Images = record.OwnImages.Length > 0 ? record.OwnImages : (property.Images ?? []);
                record.PropertyOccurredAt = property.OccurredAt;
            }
            else
            {
                record.PropertyName = fallback?.PropertyName;
                record.PropertyDescription = fallback?.PropertyDescription;
                record.City = fallback?.City;
                record.Country = fallback?.Country;
                record.Latitude = fallback?.Latitude;
                record.Longitude = fallback?.Longitude;
                record.Facilities = record.OwnFacilities;
                record.Images = record.OwnImages;
                record.PropertyOccurredAt = fallback?.PropertyOccurredAt;
            }
        }

        private async Task ApplySearchTextAndEmbeddingAsync(AccommodationIndexRecord record, AccommodationIndexRecord existing, CancellationToken cancellationToken)
        {
            record.SearchText = ComposeSearchText(record);
            var hash = ComputeHash(record.SearchText);

            if (existing is not null && existing.ContentHash == hash && existing.Embedding.Length > 0)
            {
                record.Embedding = existing.Embedding;
            }
            else
            {
                record.Embedding = await GenerateEmbeddingAsync(record.SearchText, cancellationToken);
            }

            record.ContentHash = hash;
        }

        private async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken)
        {
            var options = new EmbeddingGenerationOptions { Dimensions = EmbeddingsOptions.VectorDimensionSize };
            var embedding = await embeddingGenerator.GenerateVectorAsync(text ?? string.Empty, options, cancellationToken);

            return embedding;
        }

        /// <summary>
        /// Property-level name/description are folded in alongside the accommodation's own, 
        /// plus facilities and location, so proper nouns ("Hilton Barcelona") and
        /// free-text intent ("beach flat in Spain with a pool") both resonate - lexically via BM25 and
        /// semantically via the embedding.
        /// </summary>
        private static string ComposeSearchText(AccommodationIndexRecord record)
        {
            var description = string.Join(' ', new[] { record.PropertyDescription, record.Description }
                .Where(s => !string.IsNullOrWhiteSpace(s)));

            var location = string.Join(", ", new[] { record.City, record.Country }.Where(s => !string.IsNullOrWhiteSpace(s)));

            var sentences = new[]
            {
                record.PropertyName,
                record.Name,
                description,
                record.Facilities is { Length: > 0 } ? $"Facilities: {string.Join(", ", record.Facilities)}." : null,
                !string.IsNullOrWhiteSpace(location) ? $"Location: {location}." : null,
            };

            return string.Join(' ', sentences
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.EndsWith('.') ? s : s + "."));
        }

        private static string ComputeHash(string text)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text ?? string.Empty));

            return Convert.ToHexString(bytes);
        }

        private static SearchResult ToSearchResult(AccommodationIndexRecord record) => new()
        {
            AccommodationId = record.AccommodationId,
            PropertyId = Guid.Parse(record.PropertyId),
            Name = record.Name,
            Description = record.Description,
            Bedrooms = record.Bedrooms,
            Images = record.Images,
            Facilities = record.Facilities,
            PropertyName = record.PropertyName,
            PropertyDescription = record.PropertyDescription,
            City = record.City,
            Country = record.Country,
            Latitude = record.Latitude,
            Longitude = record.Longitude,
            MinPrice = record.MinPriceAmount.HasValue ? new Money((decimal)record.MinPriceAmount.Value, record.MinPriceCurrency) : null,
            MaxPrice = record.MaxPriceAmount.HasValue ? new Money((decimal)record.MaxPriceAmount.Value, record.MaxPriceCurrency) : null,
        };
    }
}
