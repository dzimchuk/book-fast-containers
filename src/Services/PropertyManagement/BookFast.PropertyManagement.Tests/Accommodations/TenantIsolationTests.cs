using BookFast.Common.Domain;
using BookFast.Common.TestInfrastructure;
using BookFast.PropertyManagement.Application.Accommodations.CreateAccommodation;
using BookFast.PropertyManagement.Application.Accommodations.UpdateAccommodation;
using System.Net;
using System.Net.Http.Json;

namespace BookFast.PropertyManagement.Tests.Accommodations
{
    [Collection(nameof(IntegrationTestCollection))]
    public class TenantIsolationTests(TenantIsolationFixture fixture) : IClassFixture<TenantIsolationFixture>
    {
        [Fact]
        public async Task CreateAccommodation_ReturnsNotFound()
        {
            var command = new CreateAccommodationCommand
            {
                Name = "Hijacked Room",
                Bedrooms = 1,
                Quantity = 1,
                PriceRange = new PriceRange(new Money(50m, "USD"), new Money(50m, "USD"))
            };

            var response = await fixture.HttpClient.PostAsJsonAsync($"/api/properties/{fixture.PropertyId}/accommodations", command);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task GetAccommodation_ReturnsNotFound()
        {
            var response = await fixture.HttpClient.GetAsync($"/api/properties/{fixture.PropertyId}/accommodations/{fixture.AccommodationId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task UpdateAccommodation_ReturnsNotFound()
        {
            var command = new UpdateAccommodationCommand
            {
                Name = "Hijacked",
                Bedrooms = 1,
                Quantity = 1,
                PriceRange = new PriceRange(new Money(50m, "USD"), new Money(50m, "USD"))
            };

            var response = await fixture.HttpClient.PutAsJsonAsync($"/api/properties/{fixture.PropertyId}/accommodations/{fixture.AccommodationId}", command);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task DeleteAccommodation_ReturnsNotFound()
        {
            var response = await fixture.HttpClient.DeleteAsync($"/api/properties/{fixture.PropertyId}/accommodations/{fixture.AccommodationId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task ListAccommodations_ReturnsEmpty()
        {
            var response = await fixture.HttpClient.GetAsync($"/api/properties/{fixture.PropertyId}/accommodations");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await response.ShouldBeEquivalentToFile();
        }
    }
}
