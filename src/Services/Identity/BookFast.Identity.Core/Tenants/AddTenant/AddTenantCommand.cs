using BookFast.Common.Application.Messaging;
using FluentValidation;

namespace BookFast.Identity.Core.Tenants.AddTenant
{
    public record AddTenantCommand(string Name, string TenantAdmin) : ICommand<string>
    {
    }

    public class AddTenantCommandValidator : AbstractValidator<AddTenantCommand>
    {
        public AddTenantCommandValidator()
        {
            RuleFor(commnand => commnand.Name).NotEmpty().MaximumLength(256);
            RuleFor(command => command.TenantAdmin).NotEmpty().EmailAddress().MaximumLength(256);
        }
    }
}
