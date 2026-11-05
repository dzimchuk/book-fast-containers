namespace BookFast.Common.Application.Messaging
{
    public interface ILocalBrokeredMessageSender
    {
        Task SendAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default);
    }
}
