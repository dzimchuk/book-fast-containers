using BookFast.Common.TestInfrastructure;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace BookFast.PropertyManagement.Tests.Files
{
    [Collection(nameof(IntegrationTestCollection))]
    public class IssueAccommodationFileUploadTokenTests(FileUploadFixture fixture) : IClassFixture<FileUploadFixture>
    {
        private string BaseUrl(Guid propertyId, Guid accommodationId) =>
            $"/api/properties/{propertyId}/accommodations/{accommodationId}/upload-token";

        [Fact]
        public async Task Validation()
        {
            var response = await fixture.HttpClient.GetAsync(
                BaseUrl(fixture.PropertyId, fixture.AccommodationId));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task PropertyNotFound()
        {
            var nonExistentId = Guid.NewGuid();

            var response = await fixture.HttpClient.GetAsync(
                $"{BaseUrl(nonExistentId, fixture.AccommodationId)}?originalFileName=test.jpg");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task AccommodationNotFound()
        {
            var nonExistentId = Guid.NewGuid();

            var response = await fixture.HttpClient.GetAsync(
                $"{BaseUrl(fixture.PropertyId, nonExistentId)}?originalFileName=test.jpg");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task Success()
        {
            var response = await fixture.HttpClient.GetAsync(
                $"{BaseUrl(fixture.PropertyId, fixture.AccommodationId)}?originalFileName=image.png");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonNode>();
            Assert.NotNull(json);

            var accessPermission = json["accessPermission"]?.ToString();
            Assert.Equal("Write", accessPermission);

            var url = json["url"]?.ToString();
            Assert.NotNull(url);
            Assert.Contains(fixture.PropertyId.ToString(), url);
            Assert.Contains(fixture.AccommodationId.ToString(), url);
            Assert.Contains(".png", url);

            var uri = new Uri(url);
            var query = uri.Query;
            Assert.Contains("sig=", query);
            Assert.Contains("se=", query);
            Assert.Contains("sp=", query);
        }
    }
}
