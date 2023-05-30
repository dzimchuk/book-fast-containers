namespace BookFast.PropertyManagement.Core.Commands.CreateFacility
{
    public class CreatePropertyValidator : AbstractValidator<CreatePropertyCommand>
    {
        public CreatePropertyValidator()
        {
            RuleFor(cmd => cmd.Name).NotEmpty().Length(3, 100);
            RuleFor(cmd => cmd.Description).MaximumLength(1000);

            RuleFor(cmd => cmd.Address).NotNull().SetValidator(new AddressValidator());
        }
    }
}
