namespace BookFast.SeedWork.Modeling
{
    public interface IEntity
    {
        IReadOnlyCollection<Event> Events { get; }
    }
}
