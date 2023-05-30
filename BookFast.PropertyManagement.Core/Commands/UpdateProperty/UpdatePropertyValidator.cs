namespace BookFast.PropertyManagement.Core.Commands.UpdateFacility
{
    public class UpdatePropertyValidator : AbstractValidator<UpdatePropertyCommand>
    {
        public UpdatePropertyValidator()
        {
            RuleFor(cmd => cmd.Name).NotEmpty().Length(3, 100);
            RuleFor(cmd => cmd.Description).MaximumLength(1000);

            RuleFor(cmd => cmd.Address).NotNull().SetValidator(new AddressValidator());
        }
    }
}
