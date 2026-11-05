using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using FluentValidation;

namespace BookFast.Identity.Core.TenantUsers.ChangeRole
{
    public record ChangeRoleCommand : ICommand
    {
        [SwaggerIgnore]
        public string UserId { get; set; }
        public string Role { get; init; }
    }

    public class ChangeRoleValidator : AbstractValidator<ChangeRoleCommand>
    {
        public ChangeRoleValidator()
        {
            RuleFor(command => command.UserId).NotEmpty();
            RuleFor(command => command.Role).NotEmpty()
                .Must(role => string.Equals(role, Roles.TenantAdmin, StringComparison.InvariantCultureIgnoreCase) ||
                              string.Equals(role, Roles.TenantUser, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
