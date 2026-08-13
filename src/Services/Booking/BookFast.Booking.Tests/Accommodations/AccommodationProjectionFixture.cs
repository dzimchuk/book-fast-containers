using BookFast.Booking.Domain;
using BookFast.Booking.Infrastructure.Database;
using BookFast.Common.TestInfrastructure.IntegrationTest;
using MassTransit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.Booking.Tests.Accommodations
{
    public class AccommodationProjectionFixture(BookingApiFixture fixture) : IAsyncLifetime
    {
        private WebApplicationFactory<Program> factory;

        // Publishing through the app's own DI-resolved IPublishEndpoint would route through its
        // MassTransit EF outbox which only flushes once *this app's own* DbContext SaveChanges runs
        // in the same scope, so a bare test scope's publish would silently vanish.
        //
        // A raw, independent bus connection simulates the real, separate PropertyManagement producer instead.
        private IBusControl publishBus;

        public IntegrationEventHarness IntegrationEvents => fixture.IntegrationEvents;

        public async Task InitializeAsync()
        {
            factory = fixture.GetWebApplicationFactory();

            EnsureHostStarted();

            publishBus = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri(Environment.GetEnvironmentVariable("ConnectionStrings:MessageBus")));
            });

            await publishBus.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await publishBus.StopAsync();

            await factory.DisposeAsync();
        }

        /// <summary>
        /// WebApplicationFactory builds and starts the host lazily, on first access to .Services/.CreateClient().
        /// We need to make sure the host is started before we publish events, otherwise the app's queues won't exist yet and the events will be dropped.
        /// </summary>
        private void EnsureHostStarted()
        {
            using var scope = factory.Services.CreateScope();
        }

        public Task PublishAsync<TEvent>(TEvent message, CancellationToken cancellationToken = default)
            where TEvent : class
            => publishBus.Publish(message, cancellationToken);

        public async Task<Accommodation> GetAccommodationAsync(Guid accommodationId, CancellationToken cancellationToken = default)
        {
            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BookingContext>();

            return await dbContext.Accommodations.AsNoTracking().FirstOrDefaultAsync(a => a.Id == accommodationId, cancellationToken);
        }

        public async Task<IReadOnlyCollection<Accommodation>> GetAccommodationsByPropertyAsync(Guid propertyId, CancellationToken cancellationToken = default)
        {
            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BookingContext>();

            return await dbContext.Accommodations.AsNoTracking().Where(a => a.PropertyId == propertyId).ToListAsync(cancellationToken);
        }

        public async Task<Accommodation> WaitForAccommodationAsync(Guid accommodationId, Func<Accommodation, bool> predicate, TimeSpan? timeout = null)
        {
            var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(10));

            while (DateTime.UtcNow < deadline)
            {
                var accommodation = await GetAccommodationAsync(accommodationId);
                if (accommodation is not null && predicate(accommodation))
                {
                    return accommodation;
                }

                await Task.Delay(200);
            }

            throw new TimeoutException($"No accommodation {accommodationId} matching the predicate was found within the timeout.");
        }
    }
}
