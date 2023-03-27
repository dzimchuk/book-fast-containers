namespace BookFast.Facility.Core.Commands.UpdateFacility
{
    public class UpdateFacilityValidator : AbstractValidator<UpdateFacilityCommand>
    {
        public UpdateFacilityValidator()
        {
            RuleFor(cmd => cmd.Name).NotEmpty().Length(3, 100);
            RuleFor(cmd => cmd.Description).MaximumLength(1000);
            RuleFor(cmd => cmd.StreetAddress).NotEmpty().MaximumLength(100);
        }
    }
}
