using BookFast.SeedWork;
using BookFast.Rest;
using BookFast.Web.Contracts.Security;
using BookFast.Web.Features.Booking;
using BookFast.Web.Features.Facility;
using BookFast.Web.Features.Files;
using BookFast.Web.Infrastructure.Authentication;
using BookFast.Web.Infrastructure.Authentication.Customer;
using BookFast.Web.Infrastructure.Authentication.Organizational;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using OdeToCode.AddFeatureFolders;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using Microsoft.Extensions.Configuration;

namespace BookFast.Web.Composition
{
    internal class CompositionModule : ICompositionModule
    {
        public void AddServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IAccessTokenProvider, AccessTokenProvider>();
            services.AddSingleton<ICustomerAccessTokenProvider, CustomerAccessTokenProvider>();

            var featureOptions = new FeatureFolderOptions();
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(typeof(ReauthenticationRequiredFilter));
            })
            .AddFeatureFolders(featureOptions).AddRazorOptions(o => o.ViewLocationFormats.Add(featureOptions.FeatureNamePlaceholder + "/{1}/{0}.cshtml"));

            services.AddApplicationInsightsTelemetry(configuration);

            RegisterAuthorizationPolicies(services);
            RegisterMappers(services);

            services.AddStackExchangeRedisCache(redisCacheOptions =>
            {
                redisCacheOptions.Configuration = configuration["Redis:Configuration"];
                redisCacheOptions.InstanceName = configuration["Redis:InstanceName"];
            });

            AddAuthentication(services, configuration);
        }

        private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
        {
            var authOptionsSection = configuration.GetSection("WebApp:Authentication:AzureAd");
            var b2cAuthOptionsSection = configuration.GetSection("WebApp:Authentication:AzureAd:B2C");
            var b2cPoliciesSection = configuration.GetSection("WebApp:Authentication:AzureAd:B2C:Policies");

            services.Configure<AuthenticationOptions>(authOptionsSection);
            services.Configure<B2CAuthenticationOptions>(b2cAuthOptionsSection);
            services.Configure<B2CPolicies>(b2cPoliciesSection);

            var authOptions = authOptionsSection.Get<AuthenticationOptions>();
            var b2cAuthOptions = b2cAuthOptionsSection.Get<B2CAuthenticationOptions>();
            var b2cPolicies = b2cPoliciesSection.Get<B2CPolicies>();

            var serviceProvider = services.BuildServiceProvider();
            var distributedCache = serviceProvider.GetService<IDistributedCache>();
            services.AddSingleton(distributedCache);

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = B2CAuthConstants.OpenIdConnectB2CAuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnectB2CAuthentication(b2cAuthOptions, b2cPolicies, distributedCache)
            .AddOpenIdConnectOrganizationalAuthentication(authOptions, distributedCache);
        }

        private static void RegisterAuthorizationPolicies(IServiceCollection services)
        {
            services.AddAuthorization(
                options => options.AddPolicy("FacilityProviderOnly",
                    config => config.RequireRole(InteractorRole.FacilityProvider.ToString())));

            services.AddAuthorization(options => options.AddPolicy("Customer", 
                config => config.RequireClaim(BookFastClaimTypes.InteractorRole, new[] { InteractorRole.Customer.ToString() })));
        }

        private static void RegisterMappers(IServiceCollection services)
        {
            services.AddSingleton<FacilityMapper>();
            services.AddSingleton<AccommodationMapper>();
            services.AddSingleton<BookingMapper>();
            services.AddSingleton<FileAccessMapper>();
        }
    }
}