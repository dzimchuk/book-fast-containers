using BookFast.Common.TestInfrastructure;
using BookFast.Common.TestInfrastructure.IntegrationTest;
using BookFast.PropertyManagement.Application.Accommodations.CreateAccommodation;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace BookFast.PropertyManagement.Tests.Accommodations
{
    [Collection(nameof(IntegrationTestCollection))]
    public class CreateAccommodationTests(AccommodationsFixture fixture) : IClassFixture<AccommodationsFixture>, IAsyncLifetime
    {
        private int? newAccommodationId = null;

        public static IEnumerable<object[]> ValidationData =>
        [
            ["EmptyParameters", new CreateAccommodationCommand()],
            ["NameTooShort", new CreateAccommodationCommand { Name = "ab", RoomCount = 1, Quantity = 1 }]
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
                RoomCount = 1,
                Quantity = 5,
                Price = 100m
            };

            var response = await fixture.HttpClient.PostAsJsonAsync("/api/properties/9999/accommodations", command);

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
                RoomCount = 2,
                Quantity = 3,
                Price = 200m
            };

            var response = await fixture.HttpClient.PostAsJsonAsync($"/api/properties/{fixture.PropertyId}/accommodations", command);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);

            var match = Regex.Match(response.Headers.Location.OriginalString, @".*/accommodations/(?<id>\d+)");
            Assert.True(match.Success);

            newAccommodationId = int.Parse(match.Groups["id"].Value);

            var accommodation = await fixture.DbContext.Accommodations.FindAsync(newAccommodationId.Value);
            Assert.NotNull(accommodation);
            Assert.Equal("Deluxe Suite", accommodation.Name);
            Assert.Equal("A spacious deluxe suite", accommodation.Description);
            Assert.Equal(Constants.CallerTenant, accommodation.TenantId);
            Assert.Equal(fixture.PropertyId, accommodation.PropertyId);
            Assert.True(accommodation.IsActive);
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
