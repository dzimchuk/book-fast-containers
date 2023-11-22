namespace BookFast.Identity.Core.Tenants.RemoveTenantUser
{
    public record RemoveTenantUserCommand(string UserId) : IRequest;

    public class RemoveTenantUserValidator : AbstractValidator<RemoveTenantUserCommand>
    {
        public RemoveTenantUserValidator()
        {
            RuleFor(command => command.UserId).NotEmpty();
        }
    }
}
