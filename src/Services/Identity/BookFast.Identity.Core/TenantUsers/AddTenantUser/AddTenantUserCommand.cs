using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using FluentValidation;

namespace BookFast.Identity.Core.TenantUsers.AddTenantUser
{
    public record AddTenantUserCommand(string UserName, string Role) : ICommand<string>;

    public class AddTenantUserValidator : AbstractValidator<AddTenantUserCommand>
    {
        public AddTenantUserValidator()
        {
            RuleFor(command => command.UserName).NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(command => command.Role).NotEmpty()
                .Must(role => string.Equals(role, Roles.TenantAdmin, StringComparison.InvariantCultureIgnoreCase) ||
                              string.Equals(role, Roles.TenantUser, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
