namespace BookFast.Common.Application.Integration
{
    public interface IMailMessage
    {
        public IEnumerable<string> To { get; }
        public string Subject { get; }

        object GetPayload();
        Type GetPayloadType();
    }

    public record MailMessage<TModel> : IMailMessage
    {
        public IEnumerable<string> To { get; init; }
        public string Subject { get; init; }
        public TModel Model { get; init; }

        public object GetPayload() => Model;

        public Type GetPayloadType() => typeof(TModel);
    }
}
