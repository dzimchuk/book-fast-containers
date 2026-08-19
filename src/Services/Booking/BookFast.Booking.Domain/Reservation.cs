using BookFast.Common.Domain;

namespace BookFast.Booking.Domain
{
    public class Reservation : Entity<Guid>
    {
        public string GuestId { get; private set; }

        public Guid AccommodationId { get; private set; }

        public string TenantId { get; private set; }

        public Stay Stay { get; private set; }

        public int Units { get; private set; }

        public Money Rate { get; private set; }

        public ReservationStatus Status { get; private set; }

        public DateTimeOffset ExpiresAt { get; private set; }

        public byte[] RowVersion { get; private set; }

        public Money Price => new(Rate.Amount * Stay.Nights * Units, Rate.Currency);

        public static Reservation NewReservation(
            Guid id, string guestId, Guid accommodationId, string tenantId, Stay stay, int units, Money rate, DateTimeOffset expiresAt)
        {
            return new Reservation
            {
                Id = id,
                GuestId = guestId ?? throw new ArgumentNullException(nameof(guestId)),
                AccommodationId = accommodationId,
                TenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId)),
                Stay = stay ?? throw new ArgumentNullException(nameof(stay)),
                Units = units,
                Rate = rate ?? throw new ArgumentNullException(nameof(rate)),
                Status = ReservationStatus.Pending,
                ExpiresAt = expiresAt
            };
        }

        public ReservationStatus GetDisplayStatus(DateOnly asOf) =>
            Status == ReservationStatus.Confirmed && Stay.CheckOut <= asOf
                ? ReservationStatus.Completed
                : Status;
    }
}
