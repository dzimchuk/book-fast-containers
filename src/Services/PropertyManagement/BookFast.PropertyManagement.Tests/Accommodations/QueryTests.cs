using BookFast.Common.TestInfrastructure;
using System.Net;

namespace BookFast.PropertyManagement.Tests.Accommodations
{
    [Collection(nameof(IntegrationTestCollection))]
    public class QueryTests(AccommodationsQueryFixture fixture) : IClassFixture<AccommodationsQueryFixture>
    {
        private const string baseUrl = "/api/properties";

        [Fact]
        public async Task GetAccommodation_NotFound()
        {
            var response = await fixture.HttpClient.GetAsync($"{baseUrl}/{AccommodationsQueryFixture.PropertyId}/accommodations/{Guid.Empty}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task GetAccommodation_Success()
        {
            var response = await fixture.HttpClient.GetAsync($"{baseUrl}/{AccommodationsQueryFixture.PropertyId}/accommodations/{AccommodationsQueryFixture.Accommodation1Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }

        [Fact]
        public async Task ListAccommodations_DefaultSort()
        {
            var response = await fixture.HttpClient.GetAsync($"{baseUrl}/{AccommodationsQueryFixture.PropertyId}/accommodations");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }

        [Fact]
        public async Task ListAccommodations_SortByNameDesc()
        {
            var response = await fixture.HttpClient.GetAsync($"{baseUrl}/{AccommodationsQueryFixture.PropertyId}/accommodations?orderBy=Name&orderDirection=Desc");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }

        [Fact]
        public async Task ListAccommodations_Paged()
        {
            var response = await fixture.HttpClient.GetAsync($"{baseUrl}/{AccommodationsQueryFixture.PropertyId}/accommodations?orderBy=Name&pageSize=1&pageNumber=1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }
    }
}
