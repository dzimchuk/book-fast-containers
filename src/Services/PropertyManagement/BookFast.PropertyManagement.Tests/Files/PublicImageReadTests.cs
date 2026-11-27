using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;

namespace BookFast.PropertyManagement.Tests.Files
{
    public class PublicImageReadTests(FileUploadFixture fixture) : IClassFixture<FileUploadFixture>
    {
        private static readonly byte[] ImageBytes = Encoding.UTF8.GetBytes("fake-image-content");

        [Fact]
        public async Task UploadedImageIsReadableAnonymouslyWithoutSas()
        {
            var uploadUri = await UploadPropertyImage();
            var storedUrl = new UriBuilder(uploadUri) { Query = string.Empty }.Uri;

            using var anonymousClient = new HttpClient();
            var response = await anonymousClient.GetAsync(storedUrl, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(ImageBytes, await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ContainerCannotBeListedAnonymously()
        {
            var uploadUri = await UploadPropertyImage();
            var containerUri = new Uri(uploadUri, string.Concat(uploadUri.Segments.Take(3)) + "?restype=container&comp=list");

            using var anonymousClient = new HttpClient();
            var response = await anonymousClient.GetAsync(containerUri, TestContext.Current.CancellationToken);

            Assert.True((int)response.StatusCode is >= 400 and < 500);
        }

        private async Task<Uri> UploadPropertyImage()
        {
            var tokenResponse = await fixture.HttpClient.GetAsync(
                $"/api/properties/{fixture.PropertyId}/upload-token?originalFileName=photo.jpg",
                TestContext.Current.CancellationToken);
            tokenResponse.EnsureSuccessStatusCode();

            var json = await tokenResponse.Content.ReadFromJsonAsync<JsonNode>(cancellationToken: TestContext.Current.CancellationToken);
            var uploadUri = new Uri(json!["url"]!.ToString());

            using var uploadClient = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Put, uploadUri)
            {
                Content = new ByteArrayContent(ImageBytes)
            };
            request.Headers.Add("x-ms-blob-type", "BlockBlob");
            var uploadResponse = await uploadClient.SendAsync(request, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);

            return uploadUri;
        }
    }
}
