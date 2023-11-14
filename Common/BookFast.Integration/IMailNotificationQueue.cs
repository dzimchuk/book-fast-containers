namespace BookFast.Integration
{
    public interface IMailNotificationQueue
    {
        Task EnqueueMessageAsync<TModel>(MailMessage<TModel> message, CancellationToken cancellationToken = default);
    }
}
