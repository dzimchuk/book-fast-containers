using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using BookFast.SeedWork;
using BookFast.SeedWork.Validation;

namespace BookFast.Api.ErrorHandling
{
    internal class HttpExceptionFilter : IExceptionFilter
    {
        private readonly IDictionary<Type, Action<ExceptionContext>> exceptionHandlers;

        public HttpExceptionFilter()
        {
            exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
            {
                { typeof(BusinessException), HandleBusinessException },
                { typeof(ValidationException), HandleValidationException },
                { typeof(NotFoundException), HandleNotFoundException }
            };
        }

        public void OnException(ExceptionContext context)
        {
            var type = context.Exception.GetType();
            if (exceptionHandlers.ContainsKey(type))
            {
                exceptionHandlers[type].Invoke(context);
                return;
            }

            if (type.IsSubclassOf(typeof(BusinessException)))
            {
                HandleBusinessException(context);
            }

            if (type.IsSubclassOf(typeof(NotFoundException)))
            {
                HandleNotFoundException(context);
            }
        }

        private void HandleBusinessException(ExceptionContext context)
        {
            var exception = (BusinessException)context.Exception;

            var details = new ValidationProblemDetails()
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "One or more application errors have occured."
            };

            foreach (var error in exception.Errors)
            {
                details.Errors.Add(error.Code, new[] { error.Description });
            }

            context.Result = new BadRequestObjectResult(details);

            context.ExceptionHandled = true;
        }

        private void HandleValidationException(ExceptionContext context)
        {
            var exception = (ValidationException)context.Exception;

            var details = new ValidationProblemDetails(exception.Errors)
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            };

            context.Result = new BadRequestObjectResult(details);

            context.ExceptionHandled = true;
        }

        private void HandleNotFoundException(ExceptionContext context)
        {
            var exception = (NotFoundException)context.Exception;

            var details = new ProblemDetails()
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "The specified resource was not found.",
                Detail = exception.Message
            };

            context.Result = new NotFoundObjectResult(details);

            context.ExceptionHandled = true;
        }
    }
}
