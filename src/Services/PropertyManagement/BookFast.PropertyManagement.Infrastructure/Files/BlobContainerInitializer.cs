using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Hosting;

namespace BookFast.PropertyManagement.Infrastructure.Files
{
    internal sealed class BlobContainerInitializer(BlobContainerClient containerClient) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var created = await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);
            if (created is not null)
            {
                return;
            }

            var properties = await containerClient.GetPropertiesAsync(cancellationToken: cancellationToken);
            if (properties.Value.PublicAccess != PublicAccessType.Blob)
            {
                var existingPolicies = await containerClient.GetAccessPolicyAsync(cancellationToken: cancellationToken);
                await containerClient.SetAccessPolicyAsync(PublicAccessType.Blob, existingPolicies.Value.SignedIdentifiers, cancellationToken: cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
