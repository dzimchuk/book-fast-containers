using BookFast.Common.TestInfrastructure;
using System.Net;

namespace BookFast.PropertyManagement.Tests.RentalProperties
{
    [Collection(nameof(IntegrationTestCollection))]
    public class QueryTests(RentalPropertiesQueryFixture fixture) : IClassFixture<RentalPropertiesQueryFixture>
    {
        private const string baseUrl = "/api/properties";

        [Fact]
        public async Task GetProperty_NotFound()
        {
            var response = await fixture.HttpClient.GetAsync($"{baseUrl}/{Guid.Empty}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task GetProperty_Success()
        {
            var response = await fixture.HttpClient.GetAsync($"{baseUrl}/{RentalPropertiesQueryFixture.Property1Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }

        [Fact]
        public async Task ListProperties_DefaultSort()
        {
            var response = await fixture.HttpClient.GetAsync(baseUrl);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }

        [Fact]
        public async Task ListProperties_SortByNameDesc()
        {
            var response = await fixture.HttpClient.GetAsync($"{baseUrl}?orderBy=Name&orderDirection=Desc");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }

        [Fact]
        public async Task ListProperties_Paged()
        {
            var response = await fixture.HttpClient.GetAsync($"{baseUrl}?orderBy=Name&pageSize=2&pageNumber=1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }
    }
}
