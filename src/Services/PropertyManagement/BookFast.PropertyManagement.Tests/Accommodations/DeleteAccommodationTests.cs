using BookFast.Common.TestInfrastructure;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BookFast.PropertyManagement.Tests.Accommodations
{
    [Collection(nameof(IntegrationTestCollection))]
    public class DeleteAccommodationTests(AccommodationsFixture fixture) : IClassFixture<AccommodationsFixture>
    {
        [Fact]
        public async Task AccommodationNotFound()
        {
            var response = await fixture.HttpClient.DeleteAsync($"/api/properties/{fixture.PropertyId}/accommodations/9999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task Success()
        {
            var response = await fixture.HttpClient.DeleteAsync($"/api/properties/{fixture.PropertyId}/accommodations/{fixture.AccommodationId}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var accommodation = await fixture.DbContext.Accommodations.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == fixture.AccommodationId);

            Assert.Null(accommodation);
        }
    }
}
