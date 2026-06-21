using BookFast.Common.Infrastructure;
using MassTransit;

namespace BookFast.Search.Indexer
{
    internal static class ServiceCollectionExtensions
    {
        public static void AddSearchIndexer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(configuration, ConfigureMassTransit);
        }

        private static void ConfigureMassTransit(IBusRegistrationConfigurator config)
        {
            var endpointNameFormatter = new KebabCaseEndpointNameFormatter(prefix: null, includeNamespace: false);
            config.SetEndpointNameFormatter(endpointNameFormatter);

            config.AddConsumer<AccommodationCreatedEventConsumer>();
            config.AddConsumer<AccommodationUpdatedEventConsumer>();
            config.AddConsumer<AccommodationDeletedEventConsumer>();

            config.AddConfigureEndpointsCallback((name, endpointConfig) =>
            {
                endpointConfig.UseMessageRetry(r => r.Intervals(500, 1000));
            });
        }
    }
}
