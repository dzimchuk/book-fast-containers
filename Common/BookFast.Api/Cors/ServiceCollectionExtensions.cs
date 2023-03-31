using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.Api.Cors
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCorsServices(this IServiceCollection services, IConfiguration configuration)
        {
            var corsOptions = new CorsOptions();
            configuration.GetSection("CORS").Bind(corsOptions);

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
