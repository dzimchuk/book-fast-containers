using BookFast.SeedWork.Modeling;
using System.Threading.Tasks;

namespace BookFast.ReliableEvents
{
    internal class DummyIntegrationEventPublisher : IIntegrationEventPublisher
    {
        public Task PublishAsync(IntegrationEvent @event)
        {
            return Task.CompletedTask;
        }
    }
}
