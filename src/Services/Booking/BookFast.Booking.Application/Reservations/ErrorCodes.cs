using BookFast.Common.SeedWork;

namespace BookFast.Booking.Application.Reservations
{
    internal static class ErrorCodes
    {
        public static Error AccommodationNotBookable(Guid id) =>
            Error.Problem("Reservations.AccommodationNotBookable", $"Accommodation with id {id} is not bookable.");

        public static Error InsufficientAvailability() =>
            Error.Problem("Reservations.InsufficientAvailability", "Not enough units are available for every night of the requested stay.");

        public static Error ReservationNotFound(Guid id) =>
            Error.NotFound("Reservations.ReservationNotFound", $"Reservation with id {id} not found.");
    }
}
