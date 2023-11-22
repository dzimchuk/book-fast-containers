namespace BookFast.Identity.Core.Tenants.AddTenantUser
{
    public record AddTenantUserCommand(string UserName, string Role) : IRequest<string>;

    public class AddTenantUserValidator : AbstractValidator<AddTenantUserCommand>
    {
        public AddTenantUserValidator()
        {
            RuleFor(command => command.UserName).NotNull().MaximumLength(256).EmailAddress();
            RuleFor(command => command.Role).NotEmpty().MaximumLength(256);
        }
    }
}
