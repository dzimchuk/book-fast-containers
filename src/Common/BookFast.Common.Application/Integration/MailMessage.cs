namespace BookFast.Common.Application.Integration
{
    public interface IMailMessage
    {
        public IEnumerable<string> To { get; }
        public string Subject { get; }

        object Payload { get; }
        Type PayloadType { get; }
    }

    public record MailMessage<TModel> : IMailMessage
    {
        public IEnumerable<string> To { get; init; }
        public string Subject { get; init; }
        public TModel Model { get; init; }

        public object Payload => Model;

        public Type PayloadType => typeof(TModel);
    }
}
