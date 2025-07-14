using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.Common.Api.Cors
{
    internal static class CorsExtensions
    {
        public static void AddCorsServices(this IServiceCollection services, IConfiguration configuration)
        {
            var corsOptions = new CorsOptions();
            configuration.GetSection("CORS").Bind(corsOptions);

            if (corsOptions.AllowOrigins == null || !corsOptions.AllowOrigins.Any())
            {
                return;
            }

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(corsOptions.AllowOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();

                    if (corsOptions.AllowCredentials)
                    {
                        policy.AllowCredentials();
                    }
                });
            });
        }
    }
}
