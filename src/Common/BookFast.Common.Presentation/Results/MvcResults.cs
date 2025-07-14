using BookFast.Common.SeedWork;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFast.Common.Presentation.Results
{
    public static class MvcResults
    {
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
            };

            if (extensions is not null)
            {
                foreach (var extension in extensions)
                {
                    problemDetails.Extensions.Add(extension);
                }
            }

            return new ObjectResult(details)
            {
                StatusCode = details.Status
            };
        }
    }
}
