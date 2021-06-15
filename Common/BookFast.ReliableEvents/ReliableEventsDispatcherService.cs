using BookFast.DistributedMutex;
using BookFast.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace BookFast.ReliableEvents
{
    internal class ReliableEventsDispatcherService : BackgroundService
    {
        private readonly ReliableEventsDispatcher dispatcher;
        private readonly IConfiguration configuration;
        private readonly ILogger logger;

        public ReliableEventsDispatcherService(ReliableEventsDispatcher dispatcher, 
            IConfiguration configuration, 
            ILogger<ReliableEventsDispatcherService> logger)
            : base(logger)
        {
            this.dispatcher = dispatcher;
            this.configuration = configuration;
            this.logger = logger;
        }

        protected override async Task DoRunAsync(CancellationToken stoppingToken)
        {
            var connectionString = configuration["Data:Azure:Storage:ConnectionString"];
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return;
            }

            var settings = new BlobSettings(connectionString, "mutex", configuration["General:ServiceName"]);
            var mutex = new BlobDistributedMutex(settings, dispatcher.RunDispatcherAsync, logger);

            await mutex.RunTaskWhenMutexAcquired(stoppingToken);
        }
    }
}
