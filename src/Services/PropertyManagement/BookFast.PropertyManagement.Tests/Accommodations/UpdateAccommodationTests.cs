using BookFast.Common.Domain;
using BookFast.Common.TestInfrastructure;
using BookFast.PropertyManagement.Application.Accommodations.UpdateAccommodation;
using BookFast.PropertyManagement.Domain;
using BookFast.PropertyManagement.Integration;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace BookFast.PropertyManagement.Tests.Accommodations
{
    [Collection(nameof(IntegrationTestCollection))]
    public class UpdateAccommodationTests(AccommodationsFixture fixture) : IClassFixture<AccommodationsFixture>
    {
        public static IEnumerable<object[]> ValidationData =>
        [
            ["EmptyParameters", new UpdateAccommodationCommand()],
            ["NameTooShort", new UpdateAccommodationCommand { Name = "ab", Bedrooms = 1, Quantity = 1 }],
            ["MinPriceGreaterThanMaxPrice", new UpdateAccommodationCommand
            {
                Name = "Standard Room", Quantity = 1,
                PriceRange = new PriceRange(new Money(200m, "USD"), new Money(100m, "USD"))
            }],
            ["PriceRangeCurrencyMismatch", new UpdateAccommodationCommand
            {
                Name = "Standard Room", Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(100m, "EUR"))
            }],
            ["InvalidCurrency", new UpdateAccommodationCommand
            {
                Name = "Standard Room", Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "ZZZ"), null)
            }],
            ["FacilityNotAccommodationScoped", new UpdateAccommodationCommand
            {
                Name = "Standard Room", Quantity = 1,
                Facilities = [Facility.Parking]
            }]
        ];

        [Theory, MemberData(nameof(ValidationData))]
        public async Task Validation(string caseName, UpdateAccommodationCommand command)
        {
            var response = await fixture.HttpClient.PutAsJsonAsync($"/api/properties/{fixture.PropertyId}/accommodations/{fixture.AccommodationId}", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            await response.ShouldBeEquivalentToFile(caseName: caseName, partial: true);
        }

        [Fact]
        public async Task AccommodationNotFound()
        {
            var command = new UpdateAccommodationCommand { Name = "Updated Room", Bedrooms = 1, Quantity = 1 };

            var response = await fixture.HttpClient.PutAsJsonAsync($"/api/properties/{fixture.PropertyId}/accommodations/{Guid.Empty}", command);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task Success()
        {
            var command = new UpdateAccommodationCommand
            {
                Name = "Updated Room",
                Description = "Updated description",
                Bedrooms = 2,
                Quantity = 4,
                PriceRange = new PriceRange(new Money(150m, "USD"), null)
            };

            var response = await fixture.HttpClient.PutAsJsonAsync($"/api/properties/{fixture.PropertyId}/accommodations/{fixture.AccommodationId}", command);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var accommodation = await fixture.DbContext.Accommodations.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == fixture.AccommodationId);

            Assert.NotNull(accommodation);
            Assert.Equal("Updated Room", accommodation.Name);
            Assert.Equal("Updated description", accommodation.Description);
            Assert.Equal(2, accommodation.Bedrooms);
            Assert.Equal(4, accommodation.Quantity);
            Assert.Equal(150m, accommodation.PriceRange.MinPrice.Amount);
            Assert.Null(accommodation.PriceRange.MaxPrice);

            var publishedEvent = await fixture.IntegrationEvents.WaitForEventAsync<AccommodationUpdatedEvent>(
                e => e.AccommodationId == fixture.AccommodationId && e.Name == "Updated Room");

            Assert.Equal(fixture.PropertyId, publishedEvent.PropertyId);

            JsonSerializer.SerializeToNode(publishedEvent).ShouldBeEquivalentToFile(caseName: "AccommodationUpdatedEvent", partial: true);
        }

        [Fact]
        public async Task NoPriceRange_Success()
        {
            var command = new UpdateAccommodationCommand
            {
                Name = "No Range Room",
                Bedrooms = 1,
                Quantity = 1
            };

            var response = await fixture.HttpClient.PutAsJsonAsync($"/api/properties/{fixture.PropertyId}/accommodations/{fixture.AccommodationId}", command);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var accommodation = await fixture.DbContext.Accommodations.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == fixture.AccommodationId);

            Assert.NotNull(accommodation);
            Assert.True(accommodation.PriceRange.IsEmpty);
        }

        [Fact]
        public async Task OnlyMaxPrice_Success()
        {
            var command = new UpdateAccommodationCommand
            {
                Name = "Max Only Room",
                Bedrooms = 1,
                Quantity = 1,
                PriceRange = new PriceRange(null, new Money(300m, "USD"))
            };

            var response = await fixture.HttpClient.PutAsJsonAsync($"/api/properties/{fixture.PropertyId}/accommodations/{fixture.AccommodationId}", command);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var accommodation = await fixture.DbContext.Accommodations.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == fixture.AccommodationId);

            Assert.NotNull(accommodation);
            Assert.Null(accommodation.PriceRange.MinPrice);
            Assert.Equal(300m, accommodation.PriceRange.MaxPrice.Amount);
        }

        [Fact]
        public async Task Facilities_Success()
        {
            var command = new UpdateAccommodationCommand
            {
                Name = "Updated Facilities Room",
                Bedrooms = 1,
                Quantity = 1,
                Facilities = [Facility.WiFi, Facility.Refrigerator]
            };

            var response = await fixture.HttpClient.PutAsJsonAsync($"/api/properties/{fixture.PropertyId}/accommodations/{fixture.AccommodationId}", command);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var accommodation = await fixture.DbContext.Accommodations.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == fixture.AccommodationId);

            Assert.NotNull(accommodation);
            Assert.Equal([Facility.WiFi, Facility.Refrigerator], accommodation.Facilities);
        }
    }
}
