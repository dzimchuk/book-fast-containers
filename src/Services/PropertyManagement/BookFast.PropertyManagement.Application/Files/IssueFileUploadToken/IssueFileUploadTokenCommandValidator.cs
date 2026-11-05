using FluentValidation;

namespace BookFast.PropertyManagement.Application.Files.IssueFileUploadToken
{
    internal class IssueFileUploadTokenCommandValidator : AbstractValidator<IssueFileUploadTokenCommand>
    {
        public IssueFileUploadTokenCommandValidator()
        {
            RuleFor(cmd => cmd.OriginalFileName).NotEmpty();
        }
    }
}
