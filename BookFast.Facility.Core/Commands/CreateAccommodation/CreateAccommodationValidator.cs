namespace BookFast.Facility.Core.Commands.CreateAccommodation
{
    public class CreateAccommodationValidator : AbstractValidator<CreateAccommodationCommand>
    {
        public CreateAccommodationValidator()
        {
            RuleFor(cmd => cmd.Name).NotEmpty().Length(3, 100);
            RuleFor(cmd => cmd.Description).MaximumLength(1000);
            RuleFor(cmd => cmd.RoomCount).GreaterThanOrEqualTo(1).LessThanOrEqualTo(20);
        }
    }
}
