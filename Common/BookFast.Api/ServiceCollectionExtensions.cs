using BookFast.Api.Authentication;
using BookFast.Security;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using BookFast.Api.ErrorHandling;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using BookFast.Api;

namespace BookFast.Api
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAndConfigureControllers(this IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.Filters.Add(new HttpExceptionFilter());
                options.OutputFormatters.Insert(0, new BusinessExceptionOutputFormatter());
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        }

        public static void AddAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(configuration.GetSection("Authentication:AzureAd"));

            var authOptions = configuration.GetSection("Authentication:AzureAd").Get<AuthenticationOptions>();

            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters.ValidIssuers = authOptions.ValidIssuersAsArray;
            });
        }

        public static void AddB2CAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var authOptions = configuration.GetSection("Authentication:AzureAd:B2C").Get<B2CAuthenticationOptions>();

            services.AddAuthentication(Authentication.Constants.CustomerAuthenticationScheme)
                .AddJwtBearer(Authentication.Constants.CustomerAuthenticationScheme, options =>
                {
                    options.MetadataAddress = $"{authOptions.Authority}/.well-known/openid-configuration?p={authOptions.Policy}";
                    options.Audience = authOptions.Audience;

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = ctx =>
                        {
                            var nameClaim = ctx.Principal.FindFirst("name");
                            if (nameClaim != null)
                            {
                                var claimsIdentity = (ClaimsIdentity)ctx.Principal.Identity;
                                claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, nameClaim.Value));
                            }
                            return Task.FromResult(0);
                        }
                    };
                });
        }

        public static void AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(
                options =>
                {
                    options.AddPolicy(AuthorizationPolicies.PropertyWrite, config =>
                    {
                        config.RequireRole(InteractorRole.FacilityProvider.ToString(), InteractorRole.ImporterProcess.ToString());
                    });
                });
        }
    }
}
