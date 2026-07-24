using BookFast.Common.TestInfrastructure.IntegrationTest;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace BookFast.Identity.Tests
{
    public sealed class IdentityApiFixture : ApiFixture<Program>
    {
        // Quartz.NET's own logging bridge (Quartz.Logging.LogProvider) caches a reference to
        // whichever host's ILoggerFactory first starts the scheduler in a process-wide static field.
        //
        // Since each test fixture builds and disposes its own WebApplicationFactory
        // (see ApiFixture<T>.GetWebApplicationFactory), the second host to start
        // Quartz would otherwise hit that stale, already-disposed ILoggerFactory from the first host,
        // throwing ObjectDisposedException.
        public override WebApplicationFactory<Program> GetWebApplicationFactory(Action<IServiceCollection> configureTestServices = null)
        {
            return base.GetWebApplicationFactory(services =>
            {
                var quartzHostedServiceDescriptors = services
                    .Where(d => d.ServiceType == typeof(IHostedService) && d.ImplementationType == typeof(QuartzHostedService))
                    .ToList();

                foreach (var descriptor in quartzHostedServiceDescriptors)
                {
                    services.Remove(descriptor);
                }

                configureTestServices?.Invoke(services);
            });
        }
    }
}
