using FluentValidation;

namespace BookFast.PropertyManagement.Application.Accommodations.UpdateAccommodation
{
    public class UpdateAccommodationValidator : AbstractValidator<UpdateAccommodationCommand>
    {
        public UpdateAccommodationValidator()
        {
            RuleFor(cmd => cmd.Name).NotEmpty().Length(3, 100);
            RuleFor(cmd => cmd.Description).MaximumLength(1000);
            RuleFor(cmd => cmd.Bedrooms).GreaterThanOrEqualTo(1).When(cmd => cmd.Bedrooms.HasValue);
            RuleFor(cmd => cmd.Quantity).GreaterThanOrEqualTo(1);
            RuleFor(cmd => cmd.PriceRange).SetValidator(new PriceRangeValidator()).When(cmd => cmd.PriceRange is not null);
        }
    }
}
