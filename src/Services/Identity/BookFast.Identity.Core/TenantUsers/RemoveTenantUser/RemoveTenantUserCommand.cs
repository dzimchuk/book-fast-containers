using BookFast.Common.Application.Messaging;
using FluentValidation;

namespace BookFast.Identity.Core.TenantUsers.RemoveTenantUser
{
    public record RemoveTenantUserCommand : ICommand
    {
        [SwaggerIgnore]
        public string UserId { get; init; }
    }

    public class RemoveTenantUserValidator : AbstractValidator<RemoveTenantUserCommand>
    {
        public RemoveTenantUserValidator()
        {
            RuleFor(command => command.UserId).NotEmpty();
        }
    }
}
