using BookFast.Identity.Core.Commands.ChangeRole;
using BookFast.Identity.Infrastructure;
using BookFast.Security;
using BookFast.TestInfrastructure;
using BookFast.TestInfrastructure.IntegrationTest;
using Newtonsoft.Json.Linq;
using System.Net;
using Xunit;

namespace BookFast.Identity.Tests.UserManagement
{
    public class ChangeRoleTests : IClassFixture<ApiFixture<Program, IdentityContext>>
    {
        private const string baseUrl = "/users";

        private readonly HttpClient client;
        private readonly IdentityContext dbContext;

        public ChangeRoleTests(ApiFixture<Program, IdentityContext> fixture)
        {
            client = fixture.SeedAndGetHttpClient();
            dbContext = fixture.DbContext;
        }

        [Theory, MemberData(nameof(ValidationData))]
        public async Task Validation(string caseName, ChangeRoleCommand command)
        {
            var response = await client.PutAsJsonAsync($"{baseUrl}/10", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseText = await response.Content.ReadAsStringAsync();
            var responsePayload = JObject.Parse(responseText);

            Assert.True(responsePayload.DeepEqualsWithFile(valueKey: caseName));
        }

        public static IEnumerable<object[]> ValidationData => new[]
        {
            new object[] { "EmptyParameters", new ChangeRoleCommand() },
            new object[] { "MaxLength", new ChangeRoleCommand { Role = new string('a', 257) } }
        };

        [Fact]
        public async Task UnsupportedRole()
        {
            var response = await client.PutAsJsonAsync($"{baseUrl}/10", new ChangeRoleCommand { Role = "test" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseText = await response.Content.ReadAsStringAsync();
            var responsePayload = JObject.Parse(responseText);

            Assert.True(responsePayload.DeepEqualsWithFile());
        }

        [Fact]
        public async Task UserNotFound()
        {
            var response = await client.PutAsJsonAsync($"{baseUrl}/10", new ChangeRoleCommand { Role = Roles.TenantAdmin });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseText = await response.Content.ReadAsStringAsync();
            var responsePayload = JObject.Parse(responseText);

            Assert.True(responsePayload.DeepEqualsWithFile());
        }

        [Fact]
        public async Task WrongTenant()
        {
            var response = await client.PutAsJsonAsync($"{baseUrl}/{Database.UserFromAnotherTenant}", new ChangeRoleCommand { Role = Roles.TenantAdmin });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseText = await response.Content.ReadAsStringAsync();
            var responsePayload = JObject.Parse(responseText);

            Assert.True(responsePayload.DeepEqualsWithFile());
        }

        [Fact]
        public async Task SelfRoleChange()
        {
            var response = await client.PutAsJsonAsync($"{baseUrl}/{Database.CallerTenantAdmin}", new ChangeRoleCommand { Role = Roles.TenantUser });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseText = await response.Content.ReadAsStringAsync();
            var responsePayload = JObject.Parse(responseText);

            Assert.True(responsePayload.DeepEqualsWithFile());
        }

        [Fact]
        public async Task Success()
        {
            var response = await client.PutAsJsonAsync($"{baseUrl}/{Database.CallerTenantUser}", new ChangeRoleCommand { Role = Roles.TenantAdmin });

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var role = dbContext.Roles.Single(role => role.Name == Roles.TenantAdmin);
            Assert.Contains(dbContext.UserRoles,
                userRole => userRole.RoleId == role.Id && userRole.UserId == Database.CallerTenantUser);
        }
    }
}
