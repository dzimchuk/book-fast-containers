using BookFast.Identity.Infrastructure;
using BookFast.Security;
using BookFast.TestInfrastructure.IntegrationTest;
using System.Net;
using Xunit;

namespace BookFast.Identity.Tests.TenantUserManagement
{
    public class AuthorizationTests : IClassFixture<ApiFixture<Program, IdentityContext>>
    {
        private const string baseUrl = "/users";

        private readonly HttpClient client;

        public AuthorizationTests(ApiFixture<Program, IdentityContext> fixture)
        {
            client = fixture.GetHttpClient(Roles.TenantUser);
        }

        [Theory]
        [InlineData("GET", baseUrl)]
        [InlineData("GET", $"{baseUrl}/1")]
        [InlineData("POST", baseUrl)]
        [InlineData("PUT", $"{baseUrl}/1")]
        [InlineData("DELETE", $"{baseUrl}/1")]
        public async Task Forbidden(string method, string requestUri)
        {
            var request = new HttpRequestMessage(new HttpMethod(method), requestUri);

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
