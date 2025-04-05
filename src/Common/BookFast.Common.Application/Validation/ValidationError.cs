using BookFast.Common.SeedWork;

namespace BookFast.Common.Application.Validation;

public sealed record ValidationError : Error, IErrorCollection
{
    public ValidationError(Error[] errors)
        : base(
            "General.Validation",
            "One or more validation errors occurred",
            ErrorType.Validation)
    {
        Errors = errors;
    }

    public Error[] Errors { get; }

    public static ValidationError FromResults(params Result[] results) =>
        new(results.Where(r => r.IsFailure).Select(r => r.Error).ToArray());
}
