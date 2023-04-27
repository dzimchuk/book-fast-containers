namespace BookFast.Identity.Core.Commands.ChangeRole
{
    public class ChangeRoleCommand : IRequest
    {
        [SwaggerIgnore]
        public string UserId { get; set; }
        public string Role { get; set; }
    }

    public class ChangeRoleValidator : AbstractValidator<ChangeRoleCommand>
    {
        public ChangeRoleValidator()
        {
            RuleFor(command => command.UserId).NotEmpty();
            RuleFor(command => command.Role).NotEmpty().MaximumLength(256);
        }
    }
}
