using BookFast.Common.Application.Security;
using BookFast.Common.TestInfrastructure;
using BookFast.Identity.Core.TenantUsers.ChangeRole;
using System.Net;
using System.Net.Http.Json;

namespace BookFast.Identity.Tests.TenantUsers
{
    [Collection(nameof(IntegrationTestCollection))]
    public class ChangeRoleTests(TenantUsersFixture fixture) : IClassFixture<TenantUsersFixture>
    {
        private const string baseUrl = "/api/users";

        [Theory, MemberData(nameof(ValidationData))]
        public async Task Validation(string caseName, ChangeRoleCommand command)
        {
            var response = await fixture.HttpClient.PutAsJsonAsync($"{baseUrl}/test/role", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            await response.ShouldBeEquivalentToFile(caseName: caseName);
        }

        public static IEnumerable<object[]> ValidationData =>
        [
            ["EmptyParameters", new ChangeRoleCommand()]
        ];

        [Fact]
        public async Task UnsupportedRole()
        {
            var response = await fixture.HttpClient.PutAsJsonAsync($"{baseUrl}/test/role", new ChangeRoleCommand { Role = "test" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }

        [Fact]
        public async Task UserNotFound()
        {
            var response = await fixture.HttpClient.PutAsJsonAsync($"{baseUrl}/test/role", new ChangeRoleCommand { Role = Roles.TenantAdmin });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }

        [Fact]
        public async Task WrongTenant()
        {
            var response = await fixture.HttpClient.PutAsJsonAsync($"{baseUrl}/{TenantUsersFixture.Tenant2Admin}/role", new ChangeRoleCommand { Role = Roles.TenantUser });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }

        [Fact]
        public async Task SelfRoleChange()
        {
            var response = await fixture.HttpClient.PutAsJsonAsync($"{baseUrl}/{TenantUsersFixture.Tenant1Admin}/role", new ChangeRoleCommand { Role = Roles.TenantUser });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }

        [Fact]
        public async Task Success()
        {
            var response = await fixture.HttpClient.PutAsJsonAsync($"{baseUrl}/{TenantUsersFixture.Tenant1User}/role", new ChangeRoleCommand { Role = Roles.TenantAdmin });

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var role = fixture.DbContext.Roles.Single(role => role.Name == Roles.TenantAdmin);
            Assert.Contains(fixture.DbContext.UserRoles, 
                userRole => userRole.RoleId == role.Id && userRole.UserId == TenantUsersFixture.Tenant1User);
        }
    }
}
