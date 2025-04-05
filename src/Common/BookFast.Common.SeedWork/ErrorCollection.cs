namespace BookFast.Common.SeedWork;

public interface IErrorCollection
{
    Error[] Errors { get; }
}

public sealed record ErrorCollection : Error, IErrorCollection
{
    public ErrorCollection(Error[] errors)
        : base(
            "General.MultipleErrors",
            "One or more errors occurred",
            ErrorType.Problem)
    {
        Errors = errors;
    }

    public Error[] Errors { get; }
}
