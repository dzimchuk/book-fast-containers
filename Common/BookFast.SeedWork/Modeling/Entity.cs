using System.ComponentModel.DataAnnotations.Schema;

namespace BookFast.SeedWork.Modeling
{
    public abstract class Entity<TIdentity> : IEntity
    {
        private TIdentity id = default;
        private List<Event> events;

        public TIdentity Id
        {
            get { return id; }
            set
            {
                if (!id.Equals(default(TIdentity)))
                {
                    throw new InvalidOperationException("Entity ID cannot be changed");
                }

                id = value;
            }
        }

        [NotMapped]
        public IReadOnlyCollection<Event> Events => events?.AsReadOnly();

        public void AddEvent(Event @event)
        {
            events ??= new List<Event>();
            events.Add(@event);
        }
    }
}
