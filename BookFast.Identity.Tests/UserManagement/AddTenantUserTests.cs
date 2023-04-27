using BookFast.Identity.Core.Commands.AddTenantUser;
using BookFast.Identity.Infrastructure;
using BookFast.Security;
using BookFast.TestInfrastructure;
using BookFast.TestInfrastructure.IntegrationTest;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Xunit;

namespace BookFast.Identity.Tests.UserManagement
{
    public class AddTenantUserTests : IClassFixture<ApiFixture<Program, IdentityContext>>
    {
        private const string baseUrl = "/users";

        private readonly HttpClient client;
        private readonly IdentityContext dbContext;

        public AddTenantUserTests(ApiFixture<Program, IdentityContext> fixture)
        {
            client = fixture.SeedAndGetHttpClient();
            dbContext = fixture.DbContext;
        }

        [Theory, MemberData(nameof(ValidationData))]
        public async Task Validation(string caseName, AddTenantUserCommand command)
        {
            var response = await client.PostAsJsonAsync(baseUrl, command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseText = await response.Content.ReadAsStringAsync();
            var responsePayload = JObject.Parse(responseText);

            Assert.True(responsePayload.DeepEqualsWithFile(valueKey: caseName));
        }

        public static IEnumerable<object[]> ValidationData => new[]
        {
            new object[] { "EmptyParameters", new AddTenantUserCommand(null, null) },
            new object[] { "EmailAddress", new AddTenantUserCommand(UserName: "test", null) },
            new object[] { "MaxLength", new AddTenantUserCommand(UserName: $"{new string('a', 248)}@test.com", Role: new string('a', 257)) }
        };

        [Fact]
        public async Task UnsupportedRole()
        {
            var response = await client.PostAsJsonAsync(baseUrl, new AddTenantUserCommand("test@test.com", Role: "test"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseText = await response.Content.ReadAsStringAsync();
            var responsePayload = JObject.Parse(responseText);

            Assert.True(responsePayload.DeepEqualsWithFile());
        }

        [Fact]
        public async Task UserAlreadyExists()
        {
            var response = await client.PostAsJsonAsync(baseUrl, new AddTenantUserCommand("test2@test.com", Role: Roles.TenantUser));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseText = await response.Content.ReadAsStringAsync();
            var responsePayload = JObject.Parse(responseText);

            Assert.True(responsePayload.DeepEqualsWithFile());
        }

        [Fact]
        public async Task Success()
        {
            var response = await client.PostAsJsonAsync(baseUrl, new AddTenantUserCommand("new@test.com", Role: Roles.TenantUser));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            Assert.NotNull(response.Headers.Location);

            var match = Regex.Match(response.Headers.Location.OriginalString, ".*\\/users\\/(?<id>.+)");
            Assert.True(match.Success);

            var userId = match.Groups["id"].Value;
            Assert.Contains(dbContext.Users,
                user => user.Id == userId &&
                        user.UserName == "new@test.com" &&
                        user.TenantId == Constants.CallerTenant);

            var role = dbContext.Roles.Single(role => role.Name == Roles.TenantUser);
            Assert.Contains(dbContext.UserRoles, userRole => userRole.RoleId == role.Id && userRole.UserId == userId);
        }
    }
}
