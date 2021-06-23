using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace BookFast.DistributedMutex
{
    /// <summary>
    /// adopted from https://github.com/mspnp/cloud-design-patterns/blob/master/leader-election/DistributedMutex/BlobLeaseManager.cs
    /// </summary>
    internal class BlobLeaseManager
    {
        private readonly BlobContainerClient leaseContainerClient;
        private readonly PageBlobClient leaseBlobClient;
        private readonly ILogger logger;

        public BlobLeaseManager(BlobSettings settings, ILogger logger)
            : this(settings.BlobServiceClient, settings.Container, settings.BlobName, logger)
        {
        }

        public BlobLeaseManager(BlobServiceClient blobServiceClient, string leaseContainerName, string leaseBlobName, ILogger logger)
        {
            leaseContainerClient = blobServiceClient.GetBlobContainerClient(leaseContainerName);
            leaseBlobClient = leaseContainerClient.GetPageBlobClient(leaseBlobName);
            this.logger = logger;
        }

        public async Task ReleaseLeaseAsync(string leaseId)
        {
            try
            {
                var leaseClient = leaseBlobClient.GetBlobLeaseClient(leaseId);
                await leaseClient.ReleaseAsync();
            }
            catch (RequestFailedException storageException)
            {
                // Lease will eventually be released.
                logger.LogError($"Error releasing lease. ErrorCode: {storageException.ErrorCode}. Details: {storageException}");
            }
        }

        public async Task<string> AcquireLeaseAsync(CancellationToken token)
        {
            var blobNotFound = false;
            try
            {
                var leaseClient = leaseBlobClient.GetBlobLeaseClient();
                var lease = await leaseClient.AcquireAsync(TimeSpan.FromSeconds(60), null, token);
                return lease.Value.LeaseId;
            }
            catch (RequestFailedException storageException)
            {
                var status = storageException.Status;
                if (status == (int)HttpStatusCode.NotFound)
                {
                    blobNotFound = true;
                }
                else if (status == (int)HttpStatusCode.Conflict)
                {
                    return null;
                }
                else
                {
                    logger.LogError($"Error acquiring lease. ErrorCode: {storageException.ErrorCode}. Details: {storageException}");
                }
            }

            if (blobNotFound)
            {
                await CreateBlobAsync(token);
                return await AcquireLeaseAsync(token);
            }

            return null;
        }

        public async Task<bool> RenewLeaseAsync(string leaseId, CancellationToken token)
        {
            try
            {
                var leaseClient = leaseBlobClient.GetBlobLeaseClient(leaseId);
                await leaseClient.RenewAsync(cancellationToken: token);
                return true;
            }
            catch (RequestFailedException storageException)
            {
                logger.LogError($"Error renewing lease. ErrorCode: {storageException.ErrorCode}. Details: {storageException}");

                return false;
            }
        }

        private async Task CreateBlobAsync(CancellationToken token)
        {
            try
            {
                await leaseContainerClient.CreateIfNotExistsAsync(cancellationToken: token);
                await leaseBlobClient.CreateIfNotExistsAsync(0, cancellationToken: token);
            }
            catch (RequestFailedException storageException)
            {
                logger.LogError($"Error creating a mutex blob. ErrorCode: {storageException.ErrorCode}. Details: {storageException}");
                throw;
            }
        }
    }
}
