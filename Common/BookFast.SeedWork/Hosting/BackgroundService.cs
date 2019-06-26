using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookFast.SeedWork.Hosting
{
    public abstract class BackgroundService : Microsoft.Extensions.Hosting.BackgroundService
    {
        private static readonly TimeSpan OnExceptionCoolDownInterval = TimeSpan.FromSeconds(5);

        private readonly ILogger logger;

        public BackgroundService(ILogger logger)
        {
            this.logger = logger;
        }

        protected abstract Task DoRunAsync(CancellationToken stoppingToken);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var runAgain = true;

            while (runAgain && !stoppingToken.IsCancellationRequested)
            {
                runAgain = false;

                try
                {
                    logger.LogInformation("Running...");

                    await DoRunAsync(stoppingToken);

                    logger.LogInformation("Stopping...");
                }
                catch (OperationCanceledException)
                {
                    logger.LogInformation($"Cancellation request receieved. Stopping token triggered: {stoppingToken.IsCancellationRequested}.");

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        runAgain = true;
                        await Task.Delay(OnExceptionCoolDownInterval, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"Unhandled error: {ex}");

                    runAgain = true;
                    await Task.Delay(OnExceptionCoolDownInterval, stoppingToken);
                }
            }
        }
    }
}
