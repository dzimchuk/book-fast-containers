using BookFast.Common.TestInfrastructure;
using BookFast.Identity.Core.Tenants.AddTenant;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace BookFast.Identity.Tests.Tenants
{
    [Collection(nameof(IntegrationTestCollection))]
    public class AddTenantTests(TenantsFixture fixture) : IClassFixture<TenantsFixture>, IAsyncLifetime
    {
        private string newTenantId = null;
        private string newTenantAdminId = null;

        public static IEnumerable<object[]> ValidationData =>
        [
            ["EmptyParameters", new AddTenantCommand(Name: null, TenantAdmin: null)],
            ["MaxLength", new AddTenantCommand(Name: new string('a', 257), TenantAdmin: new string('a', 257))],
            ["Email", new AddTenantCommand(Name: "Test", TenantAdmin: "test")]
        ];

        [Theory, MemberData(nameof(ValidationData))]
        public async Task Validation(string caseName, AddTenantCommand command)
        {
            var response = await fixture.HttpClient.PostAsJsonAsync("/api/tenants", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            await response.ShouldBeEquivalentToFile(caseName: caseName);
        }

        [Fact]
        public async Task TenantAlreadyExists()
        {
            var response = await fixture.HttpClient.PostAsJsonAsync("/api/tenants", new AddTenantCommand(Name: TenantsFixture.TestTenantName, TenantAdmin: "user@test.com"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }

        [Fact]
        public async Task UserAlreadyExists()
        {
            var response = await fixture.HttpClient.PostAsJsonAsync("/api/tenants", new AddTenantCommand(Name: "New tenant", TenantAdmin: "user@test.com"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }

        [Fact]
        public async Task Success()
        {
            var response = await fixture.HttpClient.PostAsJsonAsync("/api/tenants", new AddTenantCommand(Name: "New tenant", TenantAdmin: "test@test.com"));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            Assert.NotNull(response.Headers.Location);

            var match = Regex.Match(response.Headers.Location.OriginalString, ".*\\/tenants\\/(?<id>.+)");
            Assert.True(match.Success);

            newTenantId = match.Groups["id"].Value;
            var tenant = await fixture.DbContext.Tenants.FirstOrDefaultAsync(t => t.Id == newTenantId);

            Assert.NotNull(tenant);
            Assert.Equal("New tenant", tenant.Name);

            var tenantAdmin = await fixture.DbContext.Users.FirstOrDefaultAsync(user => user.UserName == "test@test.com");

            Assert.NotNull(tenantAdmin);

            newTenantAdminId = tenantAdmin.Id;
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            if (!string.IsNullOrWhiteSpace(newTenantId))
            {
                await fixture.DbContext.UserRoles.Where(role => role.UserId == newTenantAdminId).ExecuteDeleteAsync();
                await fixture.DbContext.Users.Where(user => user.Id == newTenantAdminId).ExecuteDeleteAsync();
                await fixture.DbContext.Tenants.Where(t => t.Id == newTenantId).ExecuteDeleteAsync();
            }
        }
    }
}
