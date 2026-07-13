using BookFast.Common.Domain;
using BookFast.Common.TestInfrastructure;
using BookFast.Common.TestInfrastructure.IntegrationTest;
using BookFast.PropertyManagement.Application.Accommodations.CreateAccommodation;
using BookFast.PropertyManagement.Domain;
using BookFast.PropertyManagement.Integration;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BookFast.PropertyManagement.Tests.Accommodations
{
    [Collection(nameof(IntegrationTestCollection))]
    public class CreateAccommodationTests(AccommodationsFixture fixture) : IClassFixture<AccommodationsFixture>, IAsyncLifetime
    {
        private Guid? newAccommodationId = null;

        public static IEnumerable<object[]> ValidationData =>
        [
            ["EmptyParameters", new CreateAccommodationCommand()],
            ["NameTooShort", new CreateAccommodationCommand { Name = "ab", Bedrooms = 1, Quantity = 1 }],
            ["MinPriceGreaterThanMaxPrice", new CreateAccommodationCommand
            {
                Name = "Standard Room", Quantity = 1,
                PriceRange = new PriceRange(new Money(200m, "USD"), new Money(100m, "USD"))
            }],
            ["PriceRangeCurrencyMismatch", new CreateAccommodationCommand
            {
                Name = "Standard Room", Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(100m, "EUR"))
            }],
            ["InvalidCurrency", new CreateAccommodationCommand
            {
                Name = "Standard Room", Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "ZZZ"), null)
            }],
            ["FacilityNotAccommodationScoped", new CreateAccommodationCommand
            {
                Name = "Standard Room", Quantity = 1,
                Facilities = [Facility.Parking]
            }]
        ];

        [Theory, MemberData(nameof(ValidationData))]
        public async Task Validation(string caseName, CreateAccommodationCommand command)
        {
            var response = await fixture.HttpClient.PostAsJsonAsync($"/api/properties/{fixture.PropertyId}/accommodations", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            await response.ShouldBeEquivalentToFile(caseName: caseName, partial: true);
        }

        [Fact]
        public async Task PropertyNotFound()
        {
            var command = new CreateAccommodationCommand
            {
                Name = "Standard Room",
                Bedrooms = 1,
                Quantity = 5,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(100m, "USD"))
            };

            var response = await fixture.HttpClient.PostAsJsonAsync($"/api/properties/{Guid.Empty}/accommodations", command);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task Success()
        {
            var command = new CreateAccommodationCommand
            {
                Name = "Deluxe Suite",
                Description = "A spacious deluxe suite",
                Bedrooms = 2,
                Quantity = 3,
                PriceRange = new PriceRange(new Money(150m, "USD"), new Money(200m, "USD"))
            };

            var response = await fixture.HttpClient.PostAsJsonAsync($"/api/properties/{fixture.PropertyId}/accommodations", command);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);

            var match = Regex.Match(response.Headers.Location.OriginalString, @".*/accommodations/(?<id>[0-9a-fA-F-]+)");
            Assert.True(match.Success);

            newAccommodationId = Guid.Parse(match.Groups["id"].Value);

            var accommodation = await fixture.DbContext.Accommodations.FindAsync(newAccommodationId.Value);
            Assert.NotNull(accommodation);
            Assert.Equal("Deluxe Suite", accommodation.Name);
            Assert.Equal("A spacious deluxe suite", accommodation.Description);
            Assert.Equal(2, accommodation.Bedrooms);
            Assert.Equal(150m, accommodation.PriceRange.MinPrice.Amount);
            Assert.Equal("USD", accommodation.PriceRange.MinPrice.Currency);
            Assert.Equal(200m, accommodation.PriceRange.MaxPrice.Amount);
            Assert.Equal(Constants.CallerTenant, accommodation.TenantId);
            Assert.Equal(fixture.PropertyId, accommodation.PropertyId);
            Assert.True(accommodation.IsActive);

            var publishedEvent = await fixture.IntegrationEvents.WaitForEventAsync<AccommodationCreatedEvent>(
                e => e.AccommodationId == newAccommodationId.Value);

            Assert.Equal(fixture.PropertyId, publishedEvent.PropertyId);

            JsonSerializer.SerializeToNode(publishedEvent).ShouldBeEquivalentToFile(caseName: "AccommodationCreatedEvent", partial: true);
        }

        [Fact]
        public async Task NoPriceRange_Success()
        {
            var command = new CreateAccommodationCommand
            {
                Name = "Cozy Studio",
                Quantity = 1
            };

            var response = await fixture.HttpClient.PostAsJsonAsync($"/api/properties/{fixture.PropertyId}/accommodations", command);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var match = Regex.Match(response.Headers.Location.OriginalString, @".*/accommodations/(?<id>[0-9a-fA-F-]+)");
            newAccommodationId = Guid.Parse(match.Groups["id"].Value);

            var accommodation = await fixture.DbContext.Accommodations.FindAsync(newAccommodationId.Value);
            Assert.NotNull(accommodation);
            Assert.True(accommodation.PriceRange.IsEmpty);
        }

        [Fact]
        public async Task OnlyMinPrice_Success()
        {
            var command = new CreateAccommodationCommand
            {
                Name = "Budget Room",
                Quantity = 1,
                PriceRange = new PriceRange(new Money(80m, "USD"), null)
            };

            var response = await fixture.HttpClient.PostAsJsonAsync($"/api/properties/{fixture.PropertyId}/accommodations", command);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var match = Regex.Match(response.Headers.Location.OriginalString, @".*/accommodations/(?<id>[0-9a-fA-F-]+)");
            newAccommodationId = Guid.Parse(match.Groups["id"].Value);

            var accommodation = await fixture.DbContext.Accommodations.FindAsync(newAccommodationId.Value);
            Assert.NotNull(accommodation);
            Assert.Equal(80m, accommodation.PriceRange.MinPrice.Amount);
            Assert.Null(accommodation.PriceRange.MaxPrice);
        }

        [Fact]
        public async Task OnlyMaxPrice_Success()
        {
            var command = new CreateAccommodationCommand
            {
                Name = "Premium Room",
                Quantity = 1,
                PriceRange = new PriceRange(null, new Money(300m, "USD"))
            };

            var response = await fixture.HttpClient.PostAsJsonAsync($"/api/properties/{fixture.PropertyId}/accommodations", command);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var match = Regex.Match(response.Headers.Location.OriginalString, @".*/accommodations/(?<id>[0-9a-fA-F-]+)");
            newAccommodationId = Guid.Parse(match.Groups["id"].Value);

            var accommodation = await fixture.DbContext.Accommodations.FindAsync(newAccommodationId.Value);
            Assert.NotNull(accommodation);
            Assert.Null(accommodation.PriceRange.MinPrice);
            Assert.Equal(300m, accommodation.PriceRange.MaxPrice.Amount);
        }

        [Fact]
        public async Task Facilities_Success()
        {
            var command = new CreateAccommodationCommand
            {
                Name = "Family Suite",
                Quantity = 1,
                Facilities = [Facility.WiFi, Facility.Refrigerator]
            };

            var response = await fixture.HttpClient.PostAsJsonAsync($"/api/properties/{fixture.PropertyId}/accommodations", command);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var match = Regex.Match(response.Headers.Location.OriginalString, @".*/accommodations/(?<id>[0-9a-fA-F-]+)");
            newAccommodationId = Guid.Parse(match.Groups["id"].Value);

            var accommodation = await fixture.DbContext.Accommodations.FindAsync(newAccommodationId.Value);
            Assert.NotNull(accommodation);
            Assert.Equal([Facility.WiFi, Facility.Refrigerator], accommodation.Facilities);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            if (newAccommodationId.HasValue)
            {
                await fixture.DbContext.Accommodations
                    .Where(a => a.Id == newAccommodationId.Value)
                    .ExecuteDeleteAsync();
            }
        }
    }
}
