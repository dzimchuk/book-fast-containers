using BookFast.Common.TestInfrastructure;
using BookFast.PropertyManagement.Integration;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace BookFast.PropertyManagement.Tests.Accommodations
{
    public class DeleteAccommodationTests(AccommodationsFixture fixture) : IClassFixture<AccommodationsFixture>
    {
        [Fact]
        public async Task AccommodationNotFound()
        {
            var response = await fixture.HttpClient.DeleteAsync($"/api/properties/{fixture.PropertyId}/accommodations/{Guid.Empty}", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task Success()
        {
            var response = await fixture.HttpClient.DeleteAsync($"/api/properties/{fixture.PropertyId}/accommodations/{fixture.AccommodationId}", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var accommodation = await fixture.DbContext.Accommodations.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == fixture.AccommodationId, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Null(accommodation);

            var publishedEvent = await fixture.IntegrationEvents.WaitForEventAsync<AccommodationDeletedEvent>(
                e => e.AccommodationId == fixture.AccommodationId);

            Assert.Equal(fixture.PropertyId, publishedEvent.PropertyId);
            Assert.NotEqual(Guid.Empty, publishedEvent.EventId);
            Assert.NotEqual(default, publishedEvent.OccurredAt);

            JsonSerializer.SerializeToNode(publishedEvent).ShouldBeEquivalentToFile(caseName: "AccommodationDeletedEvent", partial: true);

            Assert.False(fixture.IntegrationEvents.HasCaptured<PropertyDeactivatedEvent>(e => e.PropertyId == fixture.PropertyId));
        }
    }
}
