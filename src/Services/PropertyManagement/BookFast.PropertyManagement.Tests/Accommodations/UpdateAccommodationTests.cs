using BookFast.Common.TestInfrastructure;
using BookFast.PropertyManagement.Application.Accommodations.UpdateAccommodation;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;

namespace BookFast.PropertyManagement.Tests.Accommodations
{
    [Collection(nameof(IntegrationTestCollection))]
    public class UpdateAccommodationTests(AccommodationsFixture fixture) : IClassFixture<AccommodationsFixture>
    {
        public static IEnumerable<object[]> ValidationData =>
        [
            ["EmptyParameters", new UpdateAccommodationCommand()],
            ["NameTooShort", new UpdateAccommodationCommand { Name = "ab", RoomCount = 1, Quantity = 1 }]
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
            var command = new UpdateAccommodationCommand { Name = "Updated Room", RoomCount = 1, Quantity = 1 };

            var response = await fixture.HttpClient.PutAsJsonAsync($"/api/properties/{fixture.PropertyId}/accommodations/9999", command);

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
                RoomCount = 2,
                Quantity = 4,
                Price = 150m
            };

            var response = await fixture.HttpClient.PutAsJsonAsync($"/api/properties/{fixture.PropertyId}/accommodations/{fixture.AccommodationId}", command);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var accommodation = await fixture.DbContext.Accommodations.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == fixture.AccommodationId);

            Assert.NotNull(accommodation);
            Assert.Equal("Updated Room", accommodation.Name);
            Assert.Equal("Updated description", accommodation.Description);
            Assert.Equal(2, accommodation.RoomCount);
            Assert.Equal(4, accommodation.Quantity);
        }
    }
}
