using MediatR;
using Microsoft.Extensions.Logging;

namespace BookFast.Common.Application.ExceptionHandling
{
    internal sealed class ExceptionHandlingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<ExceptionHandlingPipelineBehavior<TRequest, TResponse>> logger;

        public ExceptionHandlingPipelineBehavior(ILogger<ExceptionHandlingPipelineBehavior<TRequest, TResponse>> logger)
        {
            this.logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unhandled exception for {RequestName}", typeof(TRequest).Name);

                throw new ApplicationException(typeof(TRequest).Name, innerException: exception);
            }
        }
    }
}
