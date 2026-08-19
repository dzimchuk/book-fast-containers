using BookFast.Booking.Domain;

namespace BookFast.Booking.Application.Reservations
{
    public class ReservationRepresentation
    {
        public Guid Id { get; set; }

        public Guid AccommodationId { get; set; }

        public DateOnly CheckIn { get; set; }

        public DateOnly CheckOut { get; set; }

        public int Units { get; set; }

        public MoneyRepresentation Rate { get; set; }

        public MoneyRepresentation Price { get; set; }

        public ReservationStatus Status { get; set; }

        public DateTimeOffset ExpiresAt { get; set; }

        public static ReservationRepresentation Map(Reservation reservation, DateOnly asOf) =>
            new()
            {
                Id = reservation.Id,
                AccommodationId = reservation.AccommodationId,
                CheckIn = reservation.Stay.CheckIn,
                CheckOut = reservation.Stay.CheckOut,
                Units = reservation.Units,
                Rate = MoneyRepresentation.Map(reservation.Rate),
                Price = MoneyRepresentation.Map(reservation.Price),
                Status = reservation.GetDisplayStatus(asOf),
                ExpiresAt = reservation.ExpiresAt
            };
    }
}
