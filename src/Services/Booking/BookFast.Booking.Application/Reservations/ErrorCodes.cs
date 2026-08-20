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

        public static Error ReservationNotPending() =>
            Error.Conflict("Reservations.ReservationNotPending", "Reservation is not awaiting payment.");

        public static Error PaymentAlreadyInProgress() =>
            Error.Conflict("Reservations.PaymentAlreadyInProgress", "A payment is already being processed for this reservation.");

        public static Error ReservationNotCancellable() =>
            Error.Conflict("Reservations.ReservationNotCancellable", "Only a confirmed reservation whose stay has not ended can be cancelled.");
    }
}
