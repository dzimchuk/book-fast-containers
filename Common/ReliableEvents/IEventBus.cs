namespace ReliableEvents
{
    public interface IEventBus
    {
        Task PublishAsync(IntegrationEvent @event);
    }
}
