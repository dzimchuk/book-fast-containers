using System.Threading.Tasks;

namespace BookFast.SeedWork.Modeling
{
    public interface IIntegrationEventPublisher
    {
        Task PublishAsync(IntegrationEvent @event);
    }
}
