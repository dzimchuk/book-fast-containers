using MassTransit;
using System.Collections.Concurrent;

namespace BookFast.Common.TestInfrastructure.IntegrationTest
{
    public class IntegrationEventHarness
    {
        private readonly List<Action<IReceiveEndpointConfigurator>> consumerRegistrations = new();
        private readonly ConcurrentBag<object> capturedEvents = new();

        private IBusControl bus;

        public IntegrationEventHarness Observe<TEvent>() where TEvent : class
        {
            consumerRegistrations.Add(endpoint => endpoint.Consumer(() => new CapturingConsumer<TEvent>(capturedEvents)));

            return this;
        }

        public async Task StartAsync()
        {
            bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri(Environment.GetEnvironmentVariable("ConnectionStrings:MessageBus")));

                cfg.ReceiveEndpoint($"test-harness-{Guid.NewGuid():N}", endpoint =>
                {
                    endpoint.Durable = false;
                    endpoint.AutoDelete = true;

                    foreach (var register in consumerRegistrations)
                    {
                        register(endpoint);
                    }
                });
            });

            await bus.StartAsync();
        }

        public async Task StopAsync()
        {
            if (bus is not null)
            {
                await bus.StopAsync();
            }
        }

        public async Task<TEvent> WaitForEventAsync<TEvent>(Func<TEvent, bool> predicate, TimeSpan? timeout = null)
            where TEvent : class
        {
            var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(10);
            var deadline = DateTime.UtcNow + effectiveTimeout;

            while (DateTime.UtcNow < deadline)
            {
                var match = capturedEvents.OfType<TEvent>().FirstOrDefault(predicate);
                if (match is not null)
                {
                    return match;
                }

                await Task.Delay(100);
            }

            throw new TimeoutException($"No {typeof(TEvent).Name} matching the predicate was captured within {effectiveTimeout}.");
        }

        public bool HasCaptured<TEvent>(Func<TEvent, bool> predicate)
            where TEvent : class
        {
            return capturedEvents.OfType<TEvent>().Any(predicate);
        }

        public int CountCaptured<TEvent>(Func<TEvent, bool> predicate)
            where TEvent : class
        {
            return capturedEvents.OfType<TEvent>().Count(predicate);
        }

        private class CapturingConsumer<TEvent>(ConcurrentBag<object> store) : IConsumer<TEvent> where TEvent : class
        {
            public Task Consume(ConsumeContext<TEvent> context)
            {
                store.Add(context.Message);

                return Task.CompletedTask;
            }
        }
    }
}
