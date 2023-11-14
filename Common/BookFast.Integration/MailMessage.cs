namespace BookFast.Integration
{
    public record MailMessage<TModel>
    {
        public IEnumerable<string> To { get; init; }
        public string Subject { get; init; }
        public TModel Model { get; init; }
    }
}
