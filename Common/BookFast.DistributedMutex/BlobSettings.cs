using Azure.Storage.Blobs;
using System;

namespace BookFast.DistributedMutex
{
    public struct BlobSettings
    {
        public string Container { get; }
        public string BlobName { get; }
        public BlobServiceClient BlobServiceClient { get; }

        public BlobSettings(string storageConnectionString, string container, string blobName)
        {
            var blobClientOptions = new BlobClientOptions();
            blobClientOptions.Retry.Delay = TimeSpan.FromSeconds(5);
            blobClientOptions.Retry.MaxRetries = 3;

            BlobServiceClient = new BlobServiceClient(storageConnectionString, blobClientOptions);
            Container = container;
            BlobName = blobName;
        }
    }
}
