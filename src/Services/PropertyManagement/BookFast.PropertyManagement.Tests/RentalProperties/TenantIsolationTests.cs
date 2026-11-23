using BookFast.Common.TestInfrastructure;
using BookFast.PropertyManagement.Application.RentalProperties.UpdateProperty;
using BookFast.PropertyManagement.Domain;
using System.Net;
using System.Net.Http.Json;

namespace BookFast.PropertyManagement.Tests.RentalProperties
{
    public class TenantIsolationTests(TenantIsolationFixture fixture) : IClassFixture<TenantIsolationFixture>
    {
        private const string baseUrl = "/api/properties";

        [Fact]
        public async Task GetProperty_ReturnsNotFound()
        {
            var response = await fixture.HttpClient.GetAsync($"{baseUrl}/{fixture.PropertyId}", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task UpdateProperty_ReturnsNotFound()
        {
            var command = new UpdatePropertyCommand
            {
                Name = "Hijacked",
                Address = new Address("USA", "Texas", "Austin", "100 Congress Ave", "78701")
            };

            var response = await fixture.HttpClient.PutAsJsonAsync($"{baseUrl}/{fixture.PropertyId}", command, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task DeleteProperty_ReturnsNotFound()
        {
            var response = await fixture.HttpClient.DeleteAsync($"{baseUrl}/{fixture.PropertyId}", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task ListProperties_ReturnsEmpty()
        {
            var response = await fixture.HttpClient.GetAsync(baseUrl, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await response.ShouldBeEquivalentToFile();
        }
    }
}
