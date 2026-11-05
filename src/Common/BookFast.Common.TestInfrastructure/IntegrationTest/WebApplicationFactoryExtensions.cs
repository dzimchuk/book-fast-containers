using MassTransit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.Common.TestInfrastructure.IntegrationTest
{
    public static class WebApplicationFactoryExtensions
    {
        /// <summary>
        /// Disposes the factory without racing MassTransit's shutdown.
        /// </summary>
        /// <remarks>
        /// Disposing a factory stops its host while the app's own <c>app.Run()</c> shutdown path stops the same host
        /// concurrently. MassTransit's stop is not safe against that (BusDepotAgentSupervisor.PreStop throws
        /// NullReferenceException).
        /// This method stops the bus depot first, then disposes the factory, avoiding the race condition.
        /// </remarks>
        public static async ValueTask DisposeSafelyAsync<TProgram>(this WebApplicationFactory<TProgram> factory)
            where TProgram : class
        {
            var depot = factory.Services.GetService<IBusDepot>();
            if (depot is not null)
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await depot.Stop(cts.Token);
            }

            await factory.DisposeAsync();
        }
    }
}
