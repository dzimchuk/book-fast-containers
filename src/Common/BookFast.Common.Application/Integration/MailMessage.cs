namespace BookFast.Common.Application.Integration
{
    public interface IMailMessage
    {
        public IEnumerable<string> To { get; }
        public string Subject { get; }

        string PayloadJson { get; }
        string PayloadType { get; }
    }

    public record MailMessage<TModel> : IMailMessage
    {
        public IEnumerable<string> To { get; init; }
        public string Subject { get; init; }
        public TModel Model { get; init; }

        public string PayloadJson => System.Text.Json.JsonSerializer.Serialize(Model);

        public string PayloadType => typeof(TModel).Name;
    }
}
