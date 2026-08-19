using FluentValidation;

namespace BookFast.Booking.Application.Reservations.CreateReservation
{
    public class CreateReservationValidator : AbstractValidator<CreateReservationCommand>
    {
        public CreateReservationValidator()
        {
            RuleFor(cmd => cmd.AccommodationId).NotEmpty();
            RuleFor(cmd => cmd.Units).GreaterThanOrEqualTo(1);

            RuleFor(cmd => cmd.CheckOut)
                .GreaterThan(cmd => cmd.CheckIn)
                .WithErrorCode("StayMustBeAtLeastOneNight")
                .WithMessage("'Check Out' must be after 'Check In'.");
        }
    }
}
