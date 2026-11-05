using Azure.Storage.Blobs;
using Microsoft.Extensions.Hosting;

namespace BookFast.PropertyManagement.Infrastructure.Files
{
    internal sealed class BlobContainerInitializer(BlobContainerClient containerClient) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken) =>
            await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
