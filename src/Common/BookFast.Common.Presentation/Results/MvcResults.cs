using BookFast.Common.SeedWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace BookFast.Common.Presentation.Results
{
    public static class MvcResults
    {
        public static IActionResult Ok() => new OkResult();
        public static IActionResult Ok(object value) => new OkObjectResult(value);
        public static IActionResult CreatedAtAction(string actionName, object routeValues, object value)
            => new CreatedAtActionResult(actionName, controllerName: null, routeValues: routeValues, value: value);
        public static IActionResult NoContent() => new NoContentResult();

        public static IActionResult Problem(Result result)
        {
            if (result.IsSuccess)
            {
                throw new InvalidOperationException();
            }

            var details = new ProblemDetails()
            {
                Title = result.Error.GetTitle(),
                Detail = result.Error.GetDetail(),
                Type = result.Error.GetErrorType(),
                Status = result.Error.GetStatusCode(),
                Extensions = result.GetExtensions()
            };

            return new ObjectResult(details)
            {
                StatusCode = details.Status
            };
        }
    }
}
