using Microsoft.WindowsAzure.Storage;

namespace BookFast.DistributedMutex
{
    public struct BlobSettings
    {
        public string Container { get; }
        public string BlobName { get; }
        public CloudStorageAccount StorageAccount { get; set; }

        public BlobSettings(CloudStorageAccount storageAccount, string container, string blobName)
        {
            StorageAccount = storageAccount;
            Container = container;
            BlobName = blobName;
        }
    }
}
