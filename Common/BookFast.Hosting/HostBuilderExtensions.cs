using Microsoft.Extensions.Hosting;

namespace BookFast.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseCustomServiceProviderFactory(this IHostBuilder builder)
        {
            // TODO: integrate with a DI container that supports covarient notification handler (needed for IntegrationEventPublisher, for instance).
            // The container should also support scoped (including asp.net core per request) lifetime.
            // See https://github.com/jbogard/MediatR/wiki/Container-Feature-Support
            return builder;
        }
    }
}
