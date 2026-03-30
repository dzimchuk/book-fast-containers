using BookFast.Common.TestInfrastructure;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace BookFast.PropertyManagement.Tests.Files
{
    [Collection(nameof(IntegrationTestCollection))]
    public class IssuePropertyFileUploadTokenTests(FileUploadFixture fixture) : IClassFixture<FileUploadFixture>
    {
        [Fact]
        public async Task Validation()
        {
            var response = await fixture.HttpClient.GetAsync(
                $"/api/properties/{fixture.PropertyId}/upload-token");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task PropertyNotFound()
        {
            var nonExistentId = Guid.NewGuid();

            var response = await fixture.HttpClient.GetAsync(
                $"/api/properties/{nonExistentId}/upload-token?originalFileName=test.jpg");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task Success()
        {
            var response = await fixture.HttpClient.GetAsync(
                $"/api/properties/{fixture.PropertyId}/upload-token?originalFileName=photo.jpg");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonNode>();
            Assert.NotNull(json);

            var accessPermission = json["accessPermission"]?.ToString();
            Assert.Equal("Write", accessPermission);

            var url = json["url"]?.ToString();
            Assert.NotNull(url);
            Assert.Contains(fixture.PropertyId.ToString(), url);
            Assert.Contains(".jpg", url);

            var uri = new Uri(url);
            var query = uri.Query;
            Assert.Contains("sig=", query);
            Assert.Contains("se=", query);
            Assert.Contains("sp=", query);
        }
    }
}
