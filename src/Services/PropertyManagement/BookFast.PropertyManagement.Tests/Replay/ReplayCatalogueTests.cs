using BookFast.PropertyManagement.Integration;
using System.Net;

namespace BookFast.PropertyManagement.Tests.Replay
{
    [Collection(nameof(IntegrationTestCollection))]
    public class ReplayCatalogueTests(ReplayCatalogueFixture fixture) : IClassFixture<ReplayCatalogueFixture>
    {
        [Fact]
        public async Task Replay_PublishesCreatedEventsForExistingData()
        {
            var response = await fixture.HttpClient.PostAsync("/api/replay", null);

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

            Assert.Equal(1, fixture.IntegrationEvents.CountCaptured<PropertyCreatedEvent>(
                e => e.PropertyId == ReplayCatalogueFixture.Property1Id));
            Assert.Equal(1, fixture.IntegrationEvents.CountCaptured<PropertyCreatedEvent>(
                e => e.PropertyId == ReplayCatalogueFixture.Property2Id));
            Assert.Equal(1, fixture.IntegrationEvents.CountCaptured<AccommodationCreatedEvent>(
                e => e.AccommodationId == ReplayCatalogueFixture.Accommodation1Id));
            Assert.Equal(1, fixture.IntegrationEvents.CountCaptured<AccommodationCreatedEvent>(
                e => e.AccommodationId == ReplayCatalogueFixture.Accommodation2Id));
        }
    }
}
