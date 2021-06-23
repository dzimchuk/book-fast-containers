using BookFast.Search.Indexer.Integration;
using BookFast.SeedWork;
using BookFast.ServiceBus;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.Search.Indexer.Composition
{
    internal class CompositionModule : ICompositionModule
    {
        public void AddServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(typeof(CompositionModule).Assembly);

            services.AddIntegrationEventReceiver(configuration, new IntegrationEventMapper());
        }
    }
}
