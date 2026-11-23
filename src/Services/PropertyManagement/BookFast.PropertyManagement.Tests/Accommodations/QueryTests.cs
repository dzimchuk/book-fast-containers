using BookFast.Common.TestInfrastructure;
using System.Net;

namespace BookFast.PropertyManagement.Tests.Accommodations
{
    public class QueryTests(AccommodationsQueryFixture fixture) : IClassFixture<AccommodationsQueryFixture>
    {
        private const string baseUrl = "/api/properties";

        [Fact]
        public async Task GetAccommodation_NotFound()
        {
            var response = await fixture.HttpClient.GetAsync($"{baseUrl}/{AccommodationsQueryFixture.PropertyId}/accommodations/{Guid.Empty}", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task GetAccommodation_Success()
        {
            var response = await fixture.HttpClient.GetAsync($"{baseUrl}/{AccommodationsQueryFixture.PropertyId}/accommodations/{AccommodationsQueryFixture.Accommodation1Id}", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }

        [Fact]
        public async Task ListAccommodations_DefaultSort()
        {
            var response = await fixture.HttpClient.GetAsync($"{baseUrl}/{AccommodationsQueryFixture.PropertyId}/accommodations", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }

        [Fact]
        public async Task ListAccommodations_SortByNameDesc()
        {
            var response = await fixture.HttpClient.GetAsync($"{baseUrl}/{AccommodationsQueryFixture.PropertyId}/accommodations?orderBy=Name&orderDirection=Desc", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }

        [Fact]
        public async Task ListAccommodations_Paged()
        {
            var response = await fixture.HttpClient.GetAsync($"{baseUrl}/{AccommodationsQueryFixture.PropertyId}/accommodations?orderBy=Name&pageSize=1&pageNumber=1", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }
    }
}
