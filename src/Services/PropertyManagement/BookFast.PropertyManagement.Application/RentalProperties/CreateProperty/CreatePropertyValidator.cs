using BookFast.PropertyManagement.Domain;
using FluentValidation;

namespace BookFast.PropertyManagement.Application.RentalProperties.CreateProperty
{
    public class CreatePropertyValidator : AbstractValidator<CreatePropertyCommand>
    {
        public CreatePropertyValidator()
        {
            RuleFor(cmd => cmd.Name).NotEmpty().Length(3, 100);
            RuleFor(cmd => cmd.Description).MaximumLength(1000);

            RuleFor(cmd => cmd.Address).NotNull().SetValidator(new AddressValidator());

            RuleForEach(cmd => cmd.Facilities)
                .Must(facility => facility.AppliesToProperty())
                .WithErrorCode("FacilityNotPropertyScoped")
                .WithMessage("'{PropertyValue}' is not a valid facility for a property.")
                .When(cmd => cmd.Facilities is not null);
        }
    }
}
