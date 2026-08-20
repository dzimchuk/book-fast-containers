using BookFast.Common.Application.Queries;
using BookFast.Common.Domain;
using BookFast.PropertyManagement.Integration;
using System.Net.Http.Json;
using System.Text.Json;

namespace BookFast.Search.Tests.Search
{
    public record SearchResultDto(
        Guid AccommodationId,
        Guid PropertyId,
        string Name,
        string Description,
        int? Bedrooms,
        string[] Images,
        string[] Facilities,
        string City,
        string Country,
        double? Latitude,
        double? Longitude,
        Money Price);

    [Collection(nameof(IntegrationTestCollection))]
    public class SearchTests(SearchFixture fixture) : IClassFixture<SearchFixture>, IAsyncLifetime
    {
        private static readonly JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web);

        public Task InitializeAsync() => fixture.ResetAsync();

        public Task DisposeAsync() => Task.CompletedTask;

        private async Task<ListQueryResult<SearchResultDto>> GetAsync(string queryString)
        {
            var response = await fixture.HttpClient.GetAsync($"/api/search?{queryString}");

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

            return await response.Content.ReadFromJsonAsync<ListQueryResult<SearchResultDto>>(jsonOptions);
        }

        private async Task<ListQueryResult<SearchResultDto>> WaitForAsync(string queryString, Func<ListQueryResult<SearchResultDto>, bool> predicate, TimeSpan? timeout = null)
        {
            var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(10));

            while (DateTime.UtcNow < deadline)
            {
                var result = await GetAsync(queryString);
                if (predicate(result))
                {
                    return result;
                }

                await Task.Delay(200);
            }

            throw new TimeoutException($"No search result matching the predicate was found for '{queryString}' within the timeout.");
        }

        private static string Keyword() => $"Kw{Guid.NewGuid():N}";

        [Fact]
        public async Task AccommodationCreated_FindableByKeywordFromNameOrDescription()
        {
            var keyword = Keyword();
            var accommodationId = Guid.NewGuid();

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = Guid.NewGuid(),
                Name = $"{keyword} Suite",
                Description = "A lovely place to stay",
                Bedrooms = 2,
                Images = [],
                Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(150m, "USD")),
                Facilities = [],
            });

            await fixture.PublishBookableAsync(accommodationId);

            var result = await WaitForAsync($"query={keyword}", r => r.Records.Any(rec => rec.AccommodationId == accommodationId));

            Assert.Contains(result.Records, r => r.AccommodationId == accommodationId && r.Name == $"{keyword} Suite");
        }

        [Fact]
        public async Task Filters_BedroomsExactMatchAndFacilities_NarrowResults()
        {
            var keyword = Keyword();
            var matchingId = Guid.NewGuid();
            var nonMatchingId = Guid.NewGuid();

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = matchingId,
                TenantId = "tenant-1",
                PropertyId = Guid.NewGuid(),
                Name = $"{keyword} Matching",
                Description = "matches all filters",
                Bedrooms = 3,
                Images = [],
                Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(150m, "USD")),
                Facilities = ["Pool", "WiFi"],
            });

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = nonMatchingId,
                TenantId = "tenant-1",
                PropertyId = Guid.NewGuid(),
                Name = $"{keyword} NonMatching",
                Description = "fails bedrooms and facilities",
                Bedrooms = 1,
                Images = [],
                Quantity = 1,
                PriceRange = new PriceRange(new Money(500m, "USD"), new Money(600m, "USD")),
                Facilities = ["Parking"],
            });

            await fixture.PublishBookableAsync(matchingId);
            await fixture.PublishBookableAsync(nonMatchingId);

            await WaitForAsync($"query={keyword}", r => r.Records.Count() == 2);

            var result = await GetAsync($"query={keyword}&bedrooms=3&facilities=Pool&facilities=WiFi");

            Assert.Single(result.Records);
            Assert.Equal(matchingId, result.Records.Single().AccommodationId);
        }

        [Fact]
        public async Task Paging_DefaultAndCustom_PagesThroughAllRecordsWithoutOverlap()
        {
            var keyword = Keyword();
            var ids = new List<Guid>();

            for (var i = 0; i < 3; i++)
            {
                var id = Guid.NewGuid();
                ids.Add(id);

                await fixture.PublishAsync(new AccommodationCreatedEvent
                {
                    AccommodationId = id,
                    TenantId = "tenant-1",
                    PropertyId = Guid.NewGuid(),
                    Name = $"{keyword} Item{i}",
                    Bedrooms = 1,
                    Images = [],
                    Quantity = 1,
                    PriceRange = new PriceRange(new Money(100m + i, "USD"), new Money(100m + i, "USD")),
                    Facilities = [],
                });

                await fixture.PublishBookableAsync(id);
            }

            await WaitForAsync($"query={keyword}", r => r.Records.Count() == 3);

            var page1 = await GetAsync($"query={keyword}&pageNumber=1&pageSize=2");
            Assert.Equal(2, page1.Records.Count());
            // MEVD has no count/total-hits API; paging via top/skip means the exact total is unknown.
            Assert.Null(page1.TotalRecords);
            Assert.Null(page1.TotalPages);

            var page2 = await GetAsync($"query={keyword}&pageNumber=2&pageSize=2");
            Assert.Single(page2.Records);
            Assert.Null(page2.TotalRecords);
            Assert.Null(page2.TotalPages);

            var pagedIds = page1.Records.Concat(page2.Records).Select(r => r.AccommodationId).ToList();
            Assert.Equal(ids.Count, pagedIds.Distinct().Count());
            Assert.Equal(ids.OrderBy(id => id), pagedIds.OrderBy(id => id));
        }

        [Fact]
        public async Task DuplicateEventId_IsNoOp()
        {
            var keyword = Keyword();
            var accommodationId = Guid.NewGuid();

            var evt = new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = Guid.NewGuid(),
                Name = $"{keyword} Once",
                Bedrooms = 1,
                Images = [],
                Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(100m, "USD")),
                Facilities = [],
            };

            await fixture.PublishAsync(evt);
            await fixture.PublishBookableAsync(accommodationId);

            await WaitForAsync($"query={keyword}", r => r.Records.Any());

            // exact duplicate redelivery: same EventId, same OccurredAt
            await fixture.PublishAsync(evt);
            await Task.Delay(500);

            var result = await GetAsync($"query={keyword}");
            Assert.Single(result.Records);
        }

        [Fact]
        public async Task OlderEventArrivingAfterNewer_IsIgnored()
        {
            var keyword = Keyword();
            var accommodationId = Guid.NewGuid();
            var propertyId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;

            await fixture.PublishAsync(new AccommodationUpdatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = $"{keyword} Newer",
                Bedrooms = 1,
                Images = [],
                Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(100m, "USD")),
                Facilities = [],
                OccurredAt = now,
            });

            await fixture.PublishBookableAsync(accommodationId);

            await WaitForAsync($"query={keyword}", r => r.Records.Any(rec => rec.Name == $"{keyword} Newer"));

            // a stale Created with an older OccurredAt arrives after the newer Updated was applied
            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = $"{keyword} Older",
                Bedrooms = 1,
                Images = [],
                Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(100m, "USD")),
                Facilities = [],
                OccurredAt = now.AddMinutes(-5),
            });

            await Task.Delay(500);

            var result = await GetAsync($"query={keyword}");
            Assert.Contains(result.Records, r => r.Name == $"{keyword} Newer");
            Assert.DoesNotContain(result.Records, r => r.Name == $"{keyword} Older");
        }

        [Fact]
        public async Task UpdatedArrivingBeforeCreated_StillIndexedCorrectly()
        {
            var keyword = Keyword();
            var accommodationId = Guid.NewGuid();

            // no Created for this id has been published: the Updated arrives "first"
            await fixture.PublishAsync(new AccommodationUpdatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = Guid.NewGuid(),
                Name = $"{keyword} FromUpdate",
                Bedrooms = 2,
                Images = [],
                Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(100m, "USD")),
                Facilities = [],
            });

            await fixture.PublishBookableAsync(accommodationId);

            var result = await WaitForAsync($"query={keyword}", r => r.Records.Any(rec => rec.AccommodationId == accommodationId));

            Assert.Contains(result.Records, r => r.AccommodationId == accommodationId && r.Name == $"{keyword} FromUpdate");
        }

        [Fact]
        public async Task Deleted_TombstonesAndLateOlderEventDoesNotResurrect()
        {
            var keyword = Keyword();
            var accommodationId = Guid.NewGuid();
            var propertyId = Guid.NewGuid();
            var tenantId = "tenant-1";
            var now = DateTimeOffset.UtcNow;

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = tenantId,
                PropertyId = propertyId,
                Name = $"{keyword} ToDelete",
                Bedrooms = 1,
                Images = [],
                Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(100m, "USD")),
                Facilities = [],
                OccurredAt = now,
            });

            await fixture.PublishBookableAsync(accommodationId);

            await WaitForAsync($"query={keyword}", r => r.Records.Any());

            await fixture.PublishAsync(new AccommodationDeletedEvent
            {
                AccommodationId = accommodationId,
                TenantId = tenantId,
                PropertyId = propertyId,
                OccurredAt = now.AddMinutes(1),
            });

            await WaitForAsync($"query={keyword}", r => !r.Records.Any());

            // a late, stale Updated (older than the delete) must not resurrect the record
            await fixture.PublishAsync(new AccommodationUpdatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = tenantId,
                PropertyId = propertyId,
                Name = $"{keyword} ResurrectionAttempt",
                Bedrooms = 1,
                Images = [],
                Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(100m, "USD")),
                Facilities = [],
                OccurredAt = now.AddSeconds(30), // older than the delete's OccurredAt, newer than the create's
            });

            await Task.Delay(500);

            var result = await GetAsync($"query={keyword}");
            Assert.Empty(result.Records);
        }

        [Fact]
        public async Task SearchResult_NeverExposesRelevanceScoreStockQuantityOrTenantId()
        {
            var keyword = Keyword();
            var accommodationId = Guid.NewGuid();

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = Guid.NewGuid(),
                Name = $"{keyword} Suite",
                Bedrooms = 1,
                Images = [],
                Quantity = 7,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(100m, "USD")),
                Facilities = [],
            });

            await fixture.PublishBookableAsync(accommodationId);

            await WaitForAsync($"query={keyword}", r => r.Records.Any());

            var response = await fixture.HttpClient.GetAsync($"/api/search?query={keyword}");
            var payload = System.Text.Json.Nodes.JsonNode.Parse(await response.Content.ReadAsStringAsync());
            var record = payload["records"].AsArray().First();
            var keys = record.AsObject().Select(p => p.Key).ToArray();

            Assert.DoesNotContain(keys, k => k.Contains("score", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(keys, k => k.Contains("quantity", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(keys, k => k.Contains("tenant", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task PropertyCreatedThenAccommodationCreated_SurfacesPropertyDerivedFields()
        {
            var keyword = Keyword();
            var propertyId = Guid.NewGuid();
            var accommodationId = Guid.NewGuid();

            await fixture.PublishAsync(new PropertyCreatedEvent
            {
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = $"{keyword} Property",
                Description = "a lovely property",
                Address = new AddressDTO("Spain", "Catalonia", "Barcelona", "Some Street 1", "08001"),
                Location = new LocationDTO(41.3874, 2.1686),
                Facilities = ["Pool"],
                Images = ["property.jpg"],
            });

            await fixture.WaitForPropertyRecordAsync(propertyId);

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = $"{keyword} Suite",
                Bedrooms = 1,
                Images = [],
                Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(100m, "USD")),
                Facilities = ["WiFi"],
            });

            await fixture.PublishBookableAsync(accommodationId);

            var result = await WaitForAsync($"query={keyword}", r => r.Records.Any(rec => rec.City == "Barcelona"));
            var record = result.Records.Single(r => r.AccommodationId == accommodationId);

            Assert.Equal("Barcelona", record.City);
            Assert.Equal("Spain", record.Country);
            Assert.Equal(41.3874, record.Latitude);
            Assert.Equal(2.1686, record.Longitude);
            Assert.Contains("Pool", record.Facilities);
            Assert.Contains("WiFi", record.Facilities);
            Assert.Equal(["property.jpg"], record.Images); // accommodation has none of its own, so it falls back to the property's
        }

        [Fact]
        public async Task PropertyUpdated_RefreshesDenormalizedFieldsOnExistingAccommodations()
        {
            var keyword = Keyword();
            var propertyId = Guid.NewGuid();
            var accommodationId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;

            await fixture.PublishAsync(new PropertyCreatedEvent
            {
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = $"{keyword} Property",
                Address = new AddressDTO("Spain", null, "Barcelona", null, null),
                Facilities = [],
                Images = [],
                OccurredAt = now,
            });

            await fixture.WaitForPropertyRecordAsync(propertyId);

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = $"{keyword} Suite",
                Bedrooms = 1,
                Images = [],
                Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(100m, "USD")),
                Facilities = [],
            });

            await fixture.PublishBookableAsync(accommodationId);

            await WaitForAsync($"query={keyword}", r => r.Records.Any(rec => rec.City == "Barcelona"));

            await fixture.PublishAsync(new PropertyUpdatedEvent
            {
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = $"{keyword} Property",
                Address = new AddressDTO("Portugal", null, "Lisbon", null, null),
                Facilities = [],
                Images = [],
                OccurredAt = now.AddMinutes(1),
            });

            var result = await WaitForAsync($"query={keyword}", r => r.Records.Any(rec => rec.City == "Lisbon"));
            var record = result.Records.Single(r => r.AccommodationId == accommodationId);

            Assert.Equal("Lisbon", record.City);
            Assert.Equal("Portugal", record.Country);
        }

        [Fact]
        public async Task CityAndCountryFilters_NarrowResults()
        {
            var keyword = Keyword();
            var barcelonaPropertyId = Guid.NewGuid();
            var lisbonPropertyId = Guid.NewGuid();
            var barcelonaAccommodationId = Guid.NewGuid();
            var lisbonAccommodationId = Guid.NewGuid();

            await fixture.PublishAsync(new PropertyCreatedEvent
            {
                TenantId = "tenant-1",
                PropertyId = barcelonaPropertyId,
                Name = $"{keyword} BarcelonaProperty",
                Address = new AddressDTO("Spain", null, "Barcelona", null, null),
                Facilities = [],
                Images = [],
            });

            await fixture.PublishAsync(new PropertyCreatedEvent
            {
                TenantId = "tenant-1",
                PropertyId = lisbonPropertyId,
                Name = $"{keyword} LisbonProperty",
                Address = new AddressDTO("Portugal", null, "Lisbon", null, null),
                Facilities = [],
                Images = [],
            });

            await fixture.WaitForPropertyRecordAsync(barcelonaPropertyId);
            await fixture.WaitForPropertyRecordAsync(lisbonPropertyId);

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = barcelonaAccommodationId,
                TenantId = "tenant-1",
                PropertyId = barcelonaPropertyId,
                Name = $"{keyword} BarcelonaSuite",
                Bedrooms = 1,
                Images = [],
                Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(100m, "USD")),
                Facilities = [],
            });

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = lisbonAccommodationId,
                TenantId = "tenant-1",
                PropertyId = lisbonPropertyId,
                Name = $"{keyword} LisbonSuite",
                Bedrooms = 1,
                Images = [],
                Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(100m, "USD")),
                Facilities = [],
            });

            await fixture.PublishBookableAsync(barcelonaAccommodationId);
            await fixture.PublishBookableAsync(lisbonAccommodationId);

            await WaitForAsync($"query={keyword}", r => r.Records.Count() == 2 && r.Records.All(rec => !string.IsNullOrEmpty(rec.City)));

            var byCity = await GetAsync($"query={keyword}&city=Barcelona");
            Assert.Single(byCity.Records);
            Assert.Equal(barcelonaAccommodationId, byCity.Records.Single().AccommodationId);

            var byCountry = await GetAsync($"query={keyword}&country=Portugal");
            Assert.Single(byCountry.Records);
            Assert.Equal(lisbonAccommodationId, byCountry.Records.Single().AccommodationId);
        }

        [Fact]
        public async Task FacilitiesFilter_MatchesWhetherRecordedOnPropertyOrAccommodation()
        {
            var keyword = Keyword();
            var propertyId = Guid.NewGuid();
            var accommodationId = Guid.NewGuid();

            await fixture.PublishAsync(new PropertyCreatedEvent
            {
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = $"{keyword} Property",
                Facilities = ["Pool"],
                Images = [],
            });

            await fixture.WaitForPropertyRecordAsync(propertyId);

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = $"{keyword} Suite",
                Bedrooms = 1,
                Images = [],
                Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(100m, "USD")),
                Facilities = ["WiFi"],
            });

            await fixture.PublishBookableAsync(accommodationId);

            await WaitForAsync($"query={keyword}", r => r.Records.Any(rec => rec.Facilities.Contains("Pool")));

            var byPropertyFacility = await GetAsync($"query={keyword}&facilities=Pool");
            Assert.Single(byPropertyFacility.Records);
            Assert.Equal(accommodationId, byPropertyFacility.Records.Single().AccommodationId);

            var byAccommodationFacility = await GetAsync($"query={keyword}&facilities=WiFi");
            Assert.Single(byAccommodationFacility.Records);
            Assert.Equal(accommodationId, byAccommodationFacility.Records.Single().AccommodationId);
        }

        [Fact]
        public async Task AccommodationCreatedBeforeProperty_IndexesThenBackfillsOncePropertyArrives()
        {
            var keyword = Keyword();
            var propertyId = Guid.NewGuid();
            var accommodationId = Guid.NewGuid();

            // no PropertyCreated for this property has been published yet
            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = $"{keyword} Suite",
                Bedrooms = 1,
                Images = [],
                Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(100m, "USD")),
                Facilities = [],
            });

            await fixture.PublishBookableAsync(accommodationId);

            var beforeBackfill = await WaitForAsync($"query={keyword}", r => r.Records.Any(rec => rec.AccommodationId == accommodationId));
            Assert.Null(beforeBackfill.Records.Single().City);

            await fixture.PublishAsync(new PropertyCreatedEvent
            {
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = $"{keyword} Property",
                Address = new AddressDTO("Spain", null, "Barcelona", null, null),
                Facilities = [],
                Images = [],
            });

            var result = await WaitForAsync($"query={keyword}", r => r.Records.Any(rec => rec.City == "Barcelona"));

            Assert.Equal(accommodationId, result.Records.Single().AccommodationId);
        }

        [Fact]
        public async Task PropertyDeactivated_ReindexesAccommodationsWithoutRemovingThem()
        {
            var keyword = Keyword();
            var propertyId = Guid.NewGuid();
            var accommodationId = Guid.NewGuid();

            await fixture.PublishAsync(new PropertyCreatedEvent
            {
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = $"{keyword} Property",
                Address = new AddressDTO("Spain", null, "Barcelona", null, null),
                Facilities = ["Pool"],
                Images = [],
            });

            await fixture.WaitForPropertyRecordAsync(propertyId);

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = $"{keyword} Suite",
                Bedrooms = 1,
                Images = [],
                Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(100m, "USD")),
                Facilities = [],
            });

            await fixture.PublishBookableAsync(accommodationId);

            await WaitForAsync($"query={keyword}", r => r.Records.Any(rec => rec.City == "Barcelona"));

            await fixture.PublishAsync(new PropertyDeactivatedEvent
            {
                TenantId = "tenant-1",
                PropertyId = propertyId,
            });

            await Task.Delay(500);

            var result = await GetAsync($"query={keyword}");
            var record = Assert.Single(result.Records);

            Assert.Equal(accommodationId, record.AccommodationId);
            Assert.Equal("Barcelona", record.City);
            Assert.Contains("Pool", record.Facilities);
        }

        [Fact]
        public async Task SemanticOnlyMatch_RiggedVectorSurfacesRecord_DespiteNoKeywordOverlap()
        {
            var keyword = Keyword();
            var accommodationId = Guid.NewGuid();
            var descriptionMarker = $"{keyword}-marker-desc";
            const string queryPhrase = "totally different words unrelated wxyz";
            var sharedVector = new float[768];
            sharedVector[0] = 1f;

            // rig the query phrase and the document's composed text (identified by the marker it contains) to
            // resolve to the SAME vector, even though they share no literal token - so a match can only come
            // from the vector arm, not BM25 keyword matching.
            fixture.RigEmbedding(queryPhrase, sharedVector);
            fixture.RigEmbedding(text => text.Contains(descriptionMarker), sharedVector);

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = Guid.NewGuid(),
                Name = $"{keyword} Suite",
                Description = descriptionMarker,
                Bedrooms = 1,
                Images = [],
                Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(100m, "USD")),
                Facilities = [],
            });

            await fixture.PublishBookableAsync(accommodationId);

            // wait for indexing via the (keyword-based) Name match, unaffected by the rigging above
            await WaitForAsync($"query={keyword}", r => r.Records.Any(rec => rec.AccommodationId == accommodationId));

            var result = await GetAsync($"query={Uri.EscapeDataString(queryPhrase)}");

            Assert.Contains(result.Records, r => r.AccommodationId == accommodationId);
        }

        [Fact]
        public async Task ReindexWithUnchangedComposedText_DoesNotRegenerateEmbedding()
        {
            var keyword = Keyword();
            var accommodationId = Guid.NewGuid();
            var propertyId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = $"{keyword} Suite",
                Description = "an unchanged description",
                Bedrooms = 1,
                Images = [],
                Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(100m, "USD")),
                Facilities = [],
                OccurredAt = now,
            });

            await fixture.PublishBookableAsync(accommodationId);

            await WaitForAsync($"query={keyword}", r => r.Records.Any(rec => rec.AccommodationId == accommodationId));

            // isolate calls embedding the DOCUMENT's composed text (identifiable by its description, which
            // never appears in the bare "{keyword}" polling query above) from calls embedding the query text
            // itself - WaitForAsync's polling re-embeds the query on every attempt, which would otherwise
            // pollute a raw call-count comparison.
            int DocumentEmbedCalls() => fixture.EmbedCallCount(c => c.Contains("an unchanged description"));

            var documentCallsAfterCreate = DocumentEmbedCalls();
            Assert.Equal(1, documentCallsAfterCreate);

            // same Name/Description/Facilities => the composed search text (and its content hash) is
            // unchanged; only Bedrooms (not part of the composed text) changes, so the update is still
            // observably applied without the embedding being regenerated.
            await fixture.PublishAsync(new AccommodationUpdatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = $"{keyword} Suite",
                Description = "an unchanged description",
                Bedrooms = 4,
                Images = [],
                Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(100m, "USD")),
                Facilities = [],
                OccurredAt = now.AddMinutes(1),
            });

            var result = await WaitForAsync($"query={keyword}", r => r.Records.Any(rec => rec.AccommodationId == accommodationId && rec.Bedrooms == 4));

            Assert.Equal(4, result.Records.Single(r => r.AccommodationId == accommodationId).Bedrooms);
            Assert.Equal(documentCallsAfterCreate, DocumentEmbedCalls());
        }

        [Fact]
        public async Task AccommodationCreated_BeforeBookableEventArrives_ExcludedFromResults()
        {
            var keyword = Keyword();
            var accommodationId = Guid.NewGuid();

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = Guid.NewGuid(),
                Name = $"{keyword} Suite",
                Bedrooms = 1,
                Images = [],
                Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(150m, "USD")),
                Facilities = [],
            });

            // no AccommodationBookableChanged has been published for this accommodation yet
            await fixture.WaitForAccommodationRecordAsync(accommodationId, r => r is not null);

            var result = await GetAsync($"query={keyword}");

            Assert.DoesNotContain(result.Records, r => r.AccommodationId == accommodationId);
        }

        [Fact]
        public async Task AccommodationBookableChanged_ShowsBookingRateInPlaceOfPropertyManagementPriceRangeMin()
        {
            var keyword = Keyword();
            var accommodationId = Guid.NewGuid();
            var bookingRate = new Money(222m, "USD");

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = Guid.NewGuid(),
                Name = $"{keyword} Suite",
                Bedrooms = 1,
                Images = [],
                Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(150m, "USD")),
                Facilities = [],
            });

            await fixture.PublishBookableAsync(accommodationId, rate: bookingRate);

            var result = await WaitForAsync($"query={keyword}", r => r.Records.Any(rec => rec.AccommodationId == accommodationId));

            Assert.Equal(bookingRate, result.Records.Single(r => r.AccommodationId == accommodationId).Price);
        }

        [Fact]
        public async Task AccommodationBookableChanged_NotBookableIsExcluded_BookableIsIncluded()
        {
            var keyword = Keyword();
            var bookableId = Guid.NewGuid();
            var notBookableId = Guid.NewGuid();

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = bookableId,
                TenantId = "tenant-1",
                PropertyId = Guid.NewGuid(),
                Name = $"{keyword} Bookable",
                Bedrooms = 1,
                Images = [],
                Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(100m, "USD")),
                Facilities = [],
            });

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = notBookableId,
                TenantId = "tenant-1",
                PropertyId = Guid.NewGuid(),
                Name = $"{keyword} NotBookable",
                Bedrooms = 1,
                Images = [],
                Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(100m, "USD")),
                Facilities = [],
            });

            await fixture.PublishBookableAsync(bookableId, bookable: true);
            await fixture.PublishBookableAsync(notBookableId, bookable: false);

            await fixture.WaitForAccommodationRecordAsync(notBookableId, r => r is not null && !r.Bookable);

            var result = await WaitForAsync($"query={keyword}", r => r.Records.Any(rec => rec.AccommodationId == bookableId));

            Assert.Contains(result.Records, r => r.AccommodationId == bookableId);
            Assert.DoesNotContain(result.Records, r => r.AccommodationId == notBookableId);
        }

        [Fact]
        public async Task AccommodationBookableChanged_DuplicateOrOutOfOrderDelivery_DoesNotCorruptTheRecord()
        {
            var keyword = Keyword();
            var accommodationId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = Guid.NewGuid(),
                Name = $"{keyword} Suite",
                Bedrooms = 1,
                Images = [],
                Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(100m, "USD")),
                Facilities = [],
            });

            var newerRate = new Money(300m, "USD");
            await fixture.PublishBookableAsync(accommodationId, rate: newerRate, occurredAt: now);

            var result = await WaitForAsync($"query={keyword}", r => r.Records.Any(rec => rec.AccommodationId == accommodationId));
            Assert.Equal(newerRate, result.Records.Single(r => r.AccommodationId == accommodationId).Price);

            // a stale, older Rate arriving after the newer one must not overwrite it
            await fixture.PublishBookableAsync(accommodationId, rate: new Money(50m, "USD"), occurredAt: now.AddMinutes(-5));
            await Task.Delay(500);

            var afterStale = await GetAsync($"query={keyword}");
            Assert.Equal(newerRate, afterStale.Records.Single(r => r.AccommodationId == accommodationId).Price);

            // an exact duplicate redelivery (same OccurredAt) is a no-op too
            await fixture.PublishBookableAsync(accommodationId, rate: newerRate, occurredAt: now);
            await Task.Delay(500);

            var afterDuplicate = await GetAsync($"query={keyword}");
            Assert.Equal(newerRate, afterDuplicate.Records.Single(r => r.AccommodationId == accommodationId).Price);
        }
    }
}
