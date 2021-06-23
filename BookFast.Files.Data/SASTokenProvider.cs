using System;
using BookFast.Files.Contracts.Models;
using Microsoft.Extensions.Options;
using BookFast.Files.Business.Data;
using Azure.Storage.Sas;
using Azure.Storage.Blobs;

namespace BookFast.Files.Data
{
    internal class SASTokenProvider : ISASTokenProvider
    {
        private readonly AzureStorageOptions storageOptions;

        public SASTokenProvider(IOptions<AzureStorageOptions> storageOptions)
        {
            this.storageOptions = storageOptions.Value;
        }

        public string GetUrlWithAccessToken(string path, AccessPermission permission, DateTimeOffset expirationTime)
        {
            var blobServiceClient = new BlobServiceClient(storageOptions.ConnectionString);
            var container = blobServiceClient.GetBlobContainerClient(storageOptions.ImageContainer);
            var blobClient = container.GetBlobClient(path);

            var sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = storageOptions.ImageContainer,
                BlobName = blobClient.Name,
                Resource = "b",
                StartsOn = DateTime.UtcNow.AddMinutes(-5),
                ExpiresOn = expirationTime
            };

            sasBuilder.SetPermissions(MapFrom(permission));

            var sasUri = blobClient.GenerateSasUri(sasBuilder);
            return sasUri.ToString();
        }

        private static BlobSasPermissions MapFrom(AccessPermission permissions)
        {
            var blobPermissions = BlobSasPermissions.Read;
            if ((permissions & AccessPermission.Read) != 0)
            {
                blobPermissions |= BlobSasPermissions.Read;
            }

            if ((permissions & AccessPermission.Write) != 0)
            {
                blobPermissions |= BlobSasPermissions.Write;
            }

            if ((permissions & AccessPermission.Delete) != 0)
            {
                blobPermissions |= BlobSasPermissions.Delete;
            }

            return blobPermissions;
        }
    }
}
