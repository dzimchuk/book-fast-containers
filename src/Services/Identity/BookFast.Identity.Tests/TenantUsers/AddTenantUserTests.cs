using BookFast.Common.Application.Security;
using BookFast.Common.TestInfrastructure;
using BookFast.Common.TestInfrastructure.IntegrationTest;
using BookFast.Identity.Core.TenantUsers.AddTenantUser;
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace BookFast.Identity.Tests.TenantUsers
{
    [Collection(nameof(IntegrationTestCollection))]
    public class AddTenantUserTests(TenantUsersFixture fixture) : IClassFixture<TenantUsersFixture>
    {
        private const string baseUrl = "/api/users";

        [Theory, MemberData(nameof(ValidationData))]
        public async Task Validation(string caseName, AddTenantUserCommand command)
        {
            var response = await fixture.HttpClient.PostAsJsonAsync(baseUrl, command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            await response.ShouldBeEquivalentToFile(caseName: caseName);
        }

        public static IEnumerable<object[]> ValidationData =>
        [
            ["EmptyParameters", new AddTenantUserCommand(UserName: null, Role: null)],
            ["EmailAddress", new AddTenantUserCommand(UserName: "test", Role: null)],
            ["MaxLength", new AddTenantUserCommand(UserName: $"{new string('a', 248)}@test.com", Role: new string('a', 257))]
        ];

        [Fact]
        public async Task UnsupportedRole()
        {
            var response = await fixture.HttpClient.PostAsJsonAsync(baseUrl, new AddTenantUserCommand(UserName: "user1@test1.com", Role: "test"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }

        [Fact]
        public async Task UserAlreadyExists()
        {
            var response = await fixture.HttpClient.PostAsJsonAsync(baseUrl, new AddTenantUserCommand(UserName: "user1@test1.com", Role: Roles.TenantUser));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }

        [Fact]
        public async Task Success()
        {
            var response = await fixture.HttpClient.PostAsJsonAsync(baseUrl, new AddTenantUserCommand(UserName: "new@test1.com", Role: Roles.TenantUser));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            Assert.NotNull(response.Headers.Location);

            var match = Regex.Match(response.Headers.Location.OriginalString, ".*\\/users\\/(?<id>.+)");
            Assert.True(match.Success);

            var userId = match.Groups["id"].Value;
            Assert.Contains(fixture.DbContext.Users, 
                user => user.Id == userId && 
                        user.UserName == "new@test1.com" &&
                        user.TenantId == Constants.CallerTenant);

            var role = fixture.DbContext.Roles.Single(role => role.Name == Roles.TenantUser);
            Assert.Contains(fixture.DbContext.UserRoles, userRole => userRole.RoleId == role.Id && userRole.UserId == userId);
        }
    }
}
