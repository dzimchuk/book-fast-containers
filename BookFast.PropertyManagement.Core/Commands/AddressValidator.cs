namespace BookFast.PropertyManagement.Core.Commands
{
    public class AddressValidator : AbstractValidator<Address>
    {
        public AddressValidator()
        {
            RuleFor(address => address.Country).NotEmpty().MaximumLength(100);
            RuleFor(address => address.State).NotEmpty().MaximumLength(100);
            RuleFor(address => address.City).NotEmpty().MaximumLength(100);
            RuleFor(address => address.Street).NotEmpty().MaximumLength(100);
            RuleFor(address => address.ZipCode).NotEmpty().MaximumLength(100);
        }
    }
}
