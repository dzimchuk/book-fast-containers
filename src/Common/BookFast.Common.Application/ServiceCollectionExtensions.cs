using BookFast.Common.Application.ExceptionHandling;
using BookFast.Common.Application.Validation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BookFast.Common.Application
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplicationServices(this IServiceCollection services,
            params Assembly[] moduleAssemblies)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(moduleAssemblies);

                cfg.AddOpenBehavior(typeof(ExceptionHandlingPipelineBehavior<,>));
                cfg.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
            });

            services.AddValidatorsFromAssemblies(moduleAssemblies, includeInternalTypes: true);
        }
    }
}
