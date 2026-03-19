using BookFast.Common.TestInfrastructure;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BookFast.PropertyManagement.Tests.RentalProperties
{
    [Collection(nameof(IntegrationTestCollection))]
    public class DeletePropertyTests(RentalPropertiesFixture fixture) : IClassFixture<RentalPropertiesFixture>
    {
        private const string baseUrl = "/api/properties";

        [Fact]
        public async Task PropertyNotFound()
        {
            var response = await fixture.HttpClient.DeleteAsync($"{baseUrl}/9999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task PropertyNotEmpty()
        {
            var response = await fixture.HttpClient.DeleteAsync($"{baseUrl}/{fixture.PropertyWithAccommodationId}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task Success()
        {
            var response = await fixture.HttpClient.DeleteAsync($"{baseUrl}/{fixture.Property2Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var property = await fixture.DbContext.Properties.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == fixture.Property2Id);

            Assert.NotNull(property);
            Assert.False(property.IsActive);
        }
    }
}
