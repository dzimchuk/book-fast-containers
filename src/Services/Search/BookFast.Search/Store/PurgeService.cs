using Microsoft.Extensions.Options;

namespace BookFast.Search.Store
{
    internal class PurgeService(
        IServiceScopeFactory scopeFactory,
        IOptions<PurgeOptions> options,
        ILogger<PurgeService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(options.Value.Interval);

            try
            {
                do
                {
                    await RunOnceAsync(stoppingToken);
                }
                while (await timer.WaitForNextTickAsync(stoppingToken));
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Purge service is stopping.");
            }
        }

        private async Task RunOnceAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var index = scope.ServiceProvider.GetRequiredService<AccommodationIndex>();

                await index.PurgeAsync(DateTimeOffset.UtcNow - options.Value.Window, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Purge run failed.");
            }
        }
    }
}
