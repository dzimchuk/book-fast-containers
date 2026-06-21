using BookFast.Common.SeedWork;

namespace BookFast.PropertyManagement.Application
{
    internal static class ErrorCodes
    {
        public static Error PropertyNotFound(Guid id) =>
            Error.NotFound("Properties.PropertyNotFound", $"Property with id {id} not found.");

        public static Error AccommodationNotFound(Guid id) =>
            Error.NotFound("Properties.AccommodationNotFound", $"Accommodation with id {id} not found.");

        public static Error PropertyNotEmpty(Guid id) =>
            Error.Problem("Properties.PropertyNotEmpty", $"Property with id {id} contains accommodations.");
    }
}
