using BookFast.PropertyManagement.Application.RentalProperties.UpdateProperty;
using BookFast.PropertyManagement.Domain;
using BookFast.PropertyManagement.Integration;
using System.Net;
using System.Net.Http.Json;

namespace BookFast.PropertyManagement.Tests.Replay
{
    [Collection(nameof(IntegrationTestCollection))]
    public class ReplayConcurrentTrafficTests(ReplayConcurrentTrafficFixture fixture) : IClassFixture<ReplayConcurrentTrafficFixture>
    {
        [Fact]
        public async Task Replay_DoesNotInterfereWithConcurrentUpdateTraffic()
        {
            var updateCommand = new UpdatePropertyCommand
            {
                Name = "Updated During Replay",
                Description = "Updated concurrently with replay",
                Address = new Address("USA", "Texas", "Austin", "100 Congress Ave", "78701")
            };

            var replayTask = fixture.HttpClient.PostAsync("/api/replay", null);
            var updateTask = fixture.HttpClient.PutAsJsonAsync(
                $"/api/properties/{ReplayConcurrentTrafficFixture.ExistingPropertyId}", updateCommand);

            await Task.WhenAll(replayTask, updateTask);

            var replayResponse = await replayTask;
            var updateResponse = await updateTask;

            Assert.Equal(HttpStatusCode.Accepted, replayResponse.StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

            var replayedEvent = await fixture.IntegrationEvents.WaitForEventAsync<PropertyCreatedEvent>(
                e => e.PropertyId == ReplayConcurrentTrafficFixture.ExistingPropertyId);
            var updatedEvent = await fixture.IntegrationEvents.WaitForEventAsync<PropertyUpdatedEvent>(
                e => e.PropertyId == ReplayConcurrentTrafficFixture.ExistingPropertyId);

            Assert.Equal(1, fixture.IntegrationEvents.CountCaptured<PropertyCreatedEvent>(
                e => e.PropertyId == ReplayConcurrentTrafficFixture.ExistingPropertyId));
            Assert.Equal(1, fixture.IntegrationEvents.CountCaptured<PropertyUpdatedEvent>(
                e => e.PropertyId == ReplayConcurrentTrafficFixture.ExistingPropertyId));

            Assert.NotEqual(replayedEvent.EventId, updatedEvent.EventId);
            Assert.Equal("Updated During Replay", updatedEvent.Name);
        }
    }
}
