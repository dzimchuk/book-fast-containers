using BookFast.Identity.Core.Tenants.ListTenantUsers;
using BookFast.Identity.Infrastructure;
using BookFast.SeedWork.Queries;
using BookFast.TestInfrastructure;
using BookFast.TestInfrastructure.IntegrationTest;
using Newtonsoft.Json.Linq;
using System.Net;
using Xunit;

namespace BookFast.Identity.Tests.TenantUserManagement
{
    public class QueryTests : IClassFixture<ApiFixture<Program, IdentityContext>>
    {
        private const string baseUrl = "/users";

        private readonly HttpClient client;

        public QueryTests(ApiFixture<Program, IdentityContext> fixture)
        {
            client = fixture.SeedAndGetHttpClient();
        }

        [Fact]
        public async Task Find_NotFound()
        {
            var response = await client.GetAsync($"{baseUrl}/10");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Find_WrongTenant()
        {
            var response = await client.GetAsync($"{baseUrl}/{Database.UserFromAnotherTenant}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Find_Success()
        {
            var result = await client.GetStringAsync($"{baseUrl}/{Database.CallerTenantAdmin}");
            Assert.True(JObject.Parse(result).DeepEqualsWithFile());
        }

        [Theory, MemberData(nameof(ListData))]
        public async Task List(string caseName, ListTenantUsersQuery query)
        {
            var parameters = query.ToUrlEncodedString();
            var url = !string.IsNullOrWhiteSpace(parameters) ? $"{baseUrl}?{parameters}" : baseUrl;

            var response = await client.GetAsync(url);

            Assert.True(response.IsSuccessStatusCode);

            var responseText = await response.Content.ReadAsStringAsync();
            var responsePayload = JObject.Parse(responseText);

            Assert.True(responsePayload.DeepEqualsWithFile(valueKey: caseName));
        }

        public static IEnumerable<object[]> ListData => new[]
        {
            new object[] { "AllUsers", new ListTenantUsersQuery() },
            new object[] { "FilterByUserName", new ListTenantUsersQuery { UserName = "test." } },
            new object[] { "FilterByRole", new ListTenantUsersQuery { Role = "use" } },
            new object[] { "FilterByUserNameAndRole", new ListTenantUsersQuery { UserName = "test.", Role = "use" } },
            new object[] { "SortByUserNameAsc", new ListTenantUsersQuery { OrderBy = "UserName" } },
            new object[] { "SortByRoleDesc", new ListTenantUsersQuery { OrderBy = "Role", OrderDirection = OrderDirection.Desc } },
            new object[] { "PagingWithSort", new ListTenantUsersQuery { OrderBy = "UserName", PageSize = 2, PageNumber = 2 } },
            new object[] { "PagingWithSortAndFilter", new ListTenantUsersQuery { Role = "user", OrderBy = "UserName", PageSize = 2, PageNumber = 1 } }
        };
    }
}
