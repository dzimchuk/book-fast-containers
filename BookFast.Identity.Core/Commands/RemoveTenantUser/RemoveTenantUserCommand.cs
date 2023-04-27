namespace BookFast.Identity.Core.Commands.RemoveTenantUser
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
