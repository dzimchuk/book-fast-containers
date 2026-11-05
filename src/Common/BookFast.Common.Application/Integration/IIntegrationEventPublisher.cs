namespace BookFast.Common.Application.Integration
{
    public interface IIntegrationEventPublisher
    {
        Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default);
    }
}
