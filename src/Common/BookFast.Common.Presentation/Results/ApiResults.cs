using BookFast.Common.SeedWork;
using Microsoft.AspNetCore.Http;

namespace BookFast.Common.Presentation.Results;

public static class ApiResults
{
    public static IResult Problem(Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException();
        }

        return Microsoft.AspNetCore.Http.Results.Problem(
            title: result.Error.GetTitle(),
            detail: result.Error.GetDetail(),
            type: result.Error.GetErrorType(),
            statusCode: result.Error.GetStatusCode(),
            extensions: result.GetExtensions());
    }
}
