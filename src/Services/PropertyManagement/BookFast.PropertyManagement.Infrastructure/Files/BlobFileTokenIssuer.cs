using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using BookFast.PropertyManagement.Application.Files;

namespace BookFast.PropertyManagement.Infrastructure.Files
{
    internal sealed class BlobFileTokenIssuer(BlobContainerClient containerClient) : IFileTokenIssuer
    {

        public FileAccessToken IssueUploadToken(string blobPath, TimeSpan validity)
        {
            var blobClient = containerClient.GetBlobClient(blobPath);

            var sasUri = blobClient.GenerateSasUri(
                BlobSasPermissions.Write | BlobSasPermissions.Create,
                DateTimeOffset.UtcNow.Add(validity));

            return new FileAccessToken
            {
                AccessPermission = AccessPermission.Write,
                Url = sasUri.ToString()
            };
        }
    }
}
