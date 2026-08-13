using BookFast.Common.Domain;

namespace BookFast.Booking.Domain
{
    public class Accommodation : Entity<Guid>
    {
        public string TenantId { get; private set; }

        public Guid PropertyId { get; private set; }

        public int Quantity { get; private set; }
        public Money Rate { get; private set; }

        public bool Active { get; private set; }

        public DateTimeOffset OccurredAt { get; private set; }

        public bool Bookable => Active && Rate is not null;

        public static Accommodation NewAccommodation(Guid accommodationId, string tenantId, Guid propertyId)
        {
            var accommodation = new Accommodation
            {
                Id = accommodationId,
                TenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId)),
                PropertyId = propertyId,
                Active = true,
                OccurredAt = DateTimeOffset.MinValue
            };

            return accommodation;
        }

        public bool ApplyCreated(int quantity, Money rate, DateTimeOffset occurredAt) => Apply(quantity, rate, forceSeed: true, occurredAt);

        public bool ApplyUpdated(int quantity, Money rate, DateTimeOffset occurredAt) => Apply(quantity, rate, forceSeed: false, occurredAt);

        public bool MarkNotBookable(DateTimeOffset occurredAt)
        {
            if (occurredAt <= OccurredAt)
            {
                return false;
            }

            var wasBookable = Bookable;

            Active = false;
            OccurredAt = occurredAt;

            return wasBookable != Bookable;
        }

        private bool Apply(int quantity, Money rate, bool forceSeed, DateTimeOffset occurredAt)
        {
            if (occurredAt <= OccurredAt)
            {
                return false;
            }

            var wasBookable = Bookable;
            var previousRate = Rate;

            Quantity = quantity;

            if (forceSeed || Rate is null)
            {
                Rate = rate ?? Rate;
            }

            OccurredAt = occurredAt;

            return wasBookable != Bookable || previousRate != Rate;
        }
    }
}
