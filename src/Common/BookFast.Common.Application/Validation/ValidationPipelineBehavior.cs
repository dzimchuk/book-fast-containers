using BookFast.Common.Application.Messaging;
using BookFast.Common.SeedWork;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using System.Reflection;

namespace BookFast.Common.Application.Validation
{
    public class ValidationPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IValidatableRequest
    {
        private readonly IEnumerable<IValidator<TRequest>> validators;

        public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            this.validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            ValidationFailure[] validationFailures = await ValidateAsync(request, cancellationToken);

            if (validationFailures.Length == 0)
            {
                return await next();
            }

            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                Type resultType = typeof(TResponse).GetGenericArguments()[0];

                MethodInfo failureMethod = typeof(Result<>)
                    .MakeGenericType(resultType)
                    .GetMethod(nameof(Result<object>.ValidationFailure));

                if (failureMethod is not null)
                {
                    return (TResponse)failureMethod.Invoke(null, [CreateValidationError(validationFailures)]);
                }
            }
            else if (typeof(TResponse) == typeof(Result))
            {
                return (TResponse)(object)Result.Failure(CreateValidationError(validationFailures));
            }

            throw new ValidationException(validationFailures);
        }

        private async Task<ValidationFailure[]> ValidateAsync(TRequest request, CancellationToken cancellationToken)
        {
            if (!validators.Any())
            {
                return [];
            }

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

            var validationFailures = validationResults
                .Where(result => !result.IsValid)
                .SelectMany(result => result.Errors)
                .ToArray();

            return validationFailures;
        }

        private static ValidationError CreateValidationError(ValidationFailure[] validationFailures) =>
            new(validationFailures.Select(f => Error.Problem(f.ErrorCode, f.ErrorMessage)).ToArray());
    }
}
