using BookFast.Identity.Infrastructure;
using BookFast.TestInfrastructure;
using BookFast.TestInfrastructure.IntegrationTest;
using Newtonsoft.Json.Linq;
using System.Net;
using Xunit;

namespace BookFast.Identity.Tests.TenantUserManagement
{
    public class RemoveTenantUserTests : IClassFixture<ApiFixture<Program, IdentityContext>>
    {
        private const string baseUrl = "/users";

        private readonly HttpClient client;
        private readonly IdentityContext dbContext;

        public RemoveTenantUserTests(ApiFixture<Program, IdentityContext> fixture)
        {
            client = fixture.SeedAndGetHttpClient();
            dbContext = fixture.DbContext;
        }

        [Fact]
        public async Task UserNotFound()
        {
            var response = await client.DeleteAsync($"{baseUrl}/10");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseText = await response.Content.ReadAsStringAsync();
            var responsePayload = JObject.Parse(responseText);

            Assert.True(responsePayload.DeepEqualsWithFile());
        }

        [Fact]
        public async Task WrongTenant()
        {
            var response = await client.DeleteAsync($"{baseUrl}/{Database.UserFromAnotherTenant}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseText = await response.Content.ReadAsStringAsync();
            var responsePayload = JObject.Parse(responseText);

            Assert.True(responsePayload.DeepEqualsWithFile());
        }

        [Fact]
        public async Task SelfRemoval()
        {
            var response = await client.DeleteAsync($"{baseUrl}/{Database.CallerTenantAdmin}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseText = await response.Content.ReadAsStringAsync();
            var responsePayload = JObject.Parse(responseText);

            Assert.True(responsePayload.DeepEqualsWithFile());
        }

        [Fact]
        public async Task Success()
        {
            var response = await client.DeleteAsync($"{baseUrl}/{Database.CallerTenantUser}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            Assert.DoesNotContain(dbContext.Users,
                user => user.Id == Database.CallerTenantUser);

            Assert.DoesNotContain(dbContext.UserRoles,
                userRole => userRole.UserId == Database.CallerTenantUser);
        }
    }
}
