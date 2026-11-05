using BookFast.Common.TestInfrastructure;
using System.Net;

namespace BookFast.Identity.Tests.TenantUsers
{
    [Collection(nameof(IntegrationTestCollection))]
    public class RemoveTenantUserTests(TenantUsersFixture fixture) : IClassFixture<TenantUsersFixture>
    {
        private const string baseUrl = "/api/users";

        [Fact]
        public async Task UserNotFound()
        {
            var response = await fixture.HttpClient.DeleteAsync($"{baseUrl}/10");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }

        [Fact]
        public async Task WrongTenant()
        {
            var response = await fixture.HttpClient.DeleteAsync($"{baseUrl}/{TenantUsersFixture.Tenant2Admin}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }

        [Fact]
        public async Task SelfRemoval()
        {
            var response = await fixture.HttpClient.DeleteAsync($"{baseUrl}/{TenantUsersFixture.Tenant1Admin}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }

        [Fact]
        public async Task Success()
        {
            var response = await fixture.HttpClient.DeleteAsync($"{baseUrl}/{TenantUsersFixture.Tenant1User}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            Assert.DoesNotContain(fixture.DbContext.Users,
                user => user.Id == TenantUsersFixture.Tenant1User);

            Assert.DoesNotContain(fixture.DbContext.UserRoles, 
                userRole => userRole.UserId == TenantUsersFixture.Tenant1User);
        }
    }
}
