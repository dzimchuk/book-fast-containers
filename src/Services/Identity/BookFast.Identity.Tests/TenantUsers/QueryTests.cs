using BookFast.Common.Application.Queries;
using BookFast.Common.TestInfrastructure;
using BookFast.Identity.Core.TenantUsers.ListTenantUsers;
using System.Net;
using System.Text.Json.Nodes;

namespace BookFast.Identity.Tests.TenantUsers
{
    [Collection(nameof(IntegrationTestCollection))]
    public class QueryTests(TenantUsersFixture fixture) : IClassFixture<TenantUsersFixture>
    {
        private const string baseUrl = "/users";

        [Fact]
        public async Task Find_NotFound()
        {
            var response = await fixture.HttpClient.GetAsync($"{baseUrl}/test");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }

        [Fact]
        public async Task Find_WrongTenant()
        {
            var response = await fixture.HttpClient.GetAsync($"{baseUrl}/{TenantUsersFixture.Tenant2Admin}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }

        [Fact]
        public async Task Find_Success()
        {
            var response = await fixture.HttpClient.GetAsync($"{baseUrl}/{TenantUsersFixture.Tenant1Admin}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await response.ShouldBeEquivalentToFile();
        }

        public static IEnumerable<object[]> ListData =>
        [
            ["AllUsers", new ListTenantUsersQuery()],
            ["FilterByUserName", new ListTenantUsersQuery { UserName = "admin" }],
            ["FilterByRole", new ListTenantUsersQuery { Role = "use" }],
            ["FilterByUserNameAndRole", new ListTenantUsersQuery { UserName = "user2", Role = "use" }],
            ["SortByUserNameAsc", new ListTenantUsersQuery { OrderBy = "UserName" }],
            ["SortByRoleDesc", new ListTenantUsersQuery { OrderBy = "Role", OrderDirection = OrderDirection.Desc }],
            ["PagingWithSort", new ListTenantUsersQuery { OrderBy = "UserName", PageSize = 2, PageNumber = 2 }],
            ["PagingWithSortAndFilter", new ListTenantUsersQuery { Role = "user", OrderBy = "UserName", PageSize = 2, PageNumber = 1 }]
        ];

        [Theory, MemberData(nameof(ListData))]
        public async Task List(string caseName, ListTenantUsersQuery query)
        {
            var parameters = query.ToUrlEncodedString();
            var url = !string.IsNullOrWhiteSpace(parameters) ? $"{baseUrl}?{parameters}" : baseUrl;

            var response = await fixture.HttpClient.GetAsync(url);

            Assert.True(response.IsSuccessStatusCode);

            await response.ShouldBeEquivalentToFile(caseName: caseName);
        }
    }
}
