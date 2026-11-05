namespace BookFast.Common.Domain
{
    public interface IEntity
    {
        IReadOnlyCollection<Event> Events { get; }
    }

    public interface IEntity<TKey> : IEntity
    {
        TKey Id { get; set; }
    }
}
