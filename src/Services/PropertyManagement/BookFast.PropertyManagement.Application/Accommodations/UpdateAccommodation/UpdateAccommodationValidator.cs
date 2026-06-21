using FluentValidation;

namespace BookFast.PropertyManagement.Application.Accommodations.UpdateAccommodation
{
    public class UpdateAccommodationValidator : AbstractValidator<UpdateAccommodationCommand>
    {
        public UpdateAccommodationValidator()
        {
            RuleFor(cmd => cmd.Name).NotEmpty().Length(3, 100);
            RuleFor(cmd => cmd.Description).MaximumLength(1000);
            RuleFor(cmd => cmd.RoomCount).GreaterThanOrEqualTo(1).LessThanOrEqualTo(20);
            RuleFor(cmd => cmd.Quantity).GreaterThanOrEqualTo(1);
            RuleFor(cmd => cmd.Price).GreaterThanOrEqualTo(decimal.Zero);
        }
    }
}
