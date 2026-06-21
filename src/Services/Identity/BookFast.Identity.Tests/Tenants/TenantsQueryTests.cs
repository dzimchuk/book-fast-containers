using BookFast.Common.TestInfrastructure;
using System.Net;

namespace BookFast.Identity.Tests.Tenants
{
    [Collection(nameof(IntegrationTestCollection))]
    public class TenantsQueryTests(TenantsFixture fixture) : IClassFixture<TenantsFixture>
    {
        [Fact]
        public async Task FindTenant_NotFound()
        {
            var response = await fixture.HttpClient.GetAsync("/api/tenants/123");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }

        [Fact]
        public async Task FindTenant_Success()
        {
            var response = await fixture.HttpClient.GetAsync($"/api/tenants/{TenantsFixture.TestTenantId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }
    }
}
