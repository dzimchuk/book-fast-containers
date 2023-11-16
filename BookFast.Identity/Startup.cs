using Azure.Storage.Blobs;
using BookFast.Api;
using BookFast.Api.Cors;
using BookFast.Api.SecurityContext;
using BookFast.Identity.Core;
using BookFast.Identity.Core.Models;
using BookFast.Identity.Infrastructure;
using BookFast.Identity.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using System.Security.Cryptography.X509Certificates;

namespace BookFast.Identity
{
    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            this.configuration = configuration;
            this.env = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationServices();

            services.AddSecurityContext();
                        
            services.AddRazorPages();
            services.AddAndConfigureControllers();

            services.AddCorsServices(configuration);

            services.AddIdentityDbContext(configuration);

            services.AddMassTransit(configuration, env, null);
            services.AddScoped<TransactionHelper>();

            services.AddApplicationInsightsTelemetry();

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services.AddTransient<CustomPasswordResetTokenProvider<User>>();
            services.AddTransient<CustomEmailConfirmationTokenProvider<User>>();

            services.AddDefaultIdentity<User>(options =>
             {
                 options.SignIn.RequireConfirmedAccount = true;

                 options.Password.RequiredLength = 8;
                 options.Password.RequireDigit = true;
                 options.Password.RequireUppercase = true;
                 options.Password.RequireLowercase = true;
                 options.Password.RequireNonAlphanumeric = false;

                 // Default token lifespan of the Identity user tokens is one day
                 // https://github.com/dotnet/aspnetcore/blob/v8.0.0/src/Identity/Extensions.Core/src/TokenOptions.cs
                 // https://github.com/dotnet/aspnetcore/blob/v8.0.0/src/Identity/Core/src/DataProtectionTokenProviderOptions.cs
                 // https://github.com/dotnet/aspnetcore/blob/v8.0.0/src/Identity/Core/src/DataProtectorTokenProvider.cs
                 options.Tokens.ProviderMap.Add("CustomPasswordReset",
                     new TokenProviderDescriptor(typeof(CustomPasswordResetTokenProvider<User>)));
                 options.Tokens.PasswordResetTokenProvider = "CustomPasswordReset";

                 options.Tokens.ProviderMap.Add("CustomEmailConfirmation",
                     new TokenProviderDescriptor(typeof(CustomEmailConfirmationTokenProvider<User>)));
                 options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";

                 // Default Lockout settings.
                 options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                 options.Lockout.MaxFailedAccessAttempts = 5;
                 options.Lockout.AllowedForNewUsers = true;
             })
             .AddErrorDescriber<CustomIdentityErrorDescriber>()
             .AddPasswordValidator<CustomPasswordValidator>()
             .AddRoles<Role>()
             .AddIdentityStore();

            var authServerSettings = configuration.GetAuthServerSettings();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.IsEssential = true;
                options.Cookie.HttpOnly = true;

                // these are set automatically based on 'Remember Me' flag
                //options.ExpireTimeSpan = TimeSpan.FromMinutes(authServerSettings.CookieLifetimeInMinutes);
                //options.Cookie.MaxAge = TimeSpan.FromMinutes(authServerSettings.CookieLifetimeInMinutes);

                options.SlidingExpiration = authServerSettings.CookieSlidingExpiration;
            });

            var dataProtectionSettings = configuration.GetDataProtectionSettings();
            if (!string.IsNullOrWhiteSpace(dataProtectionSettings.StorageConnectionString))
            {
                var container = new BlobContainerClient(dataProtectionSettings.StorageConnectionString, "data-protection");
                container.CreateIfNotExistsAsync().GetAwaiter().GetResult();

                var blob = container.GetBlobClient("identity-keys.xml");

                services.AddDataProtection()
                    .SetApplicationName("BookFast")
                    .PersistKeysToAzureBlobStorage(blob);
            }

            // OpenIddict offers native integration with Quartz.NET to perform scheduled tasks
            // (like pruning orphaned authorizations/tokens from the database) at regular intervals.
            services.AddQuartz(options =>
            {
                options.UseSimpleTypeLoader();
                options.UseInMemoryStore();
            });

            // Register the Quartz.NET service and configure it to block shutdown until jobs are complete.
            services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

            services.AddOpenIddict()

                // Register the OpenIddict core components.
                .AddCore(options =>
                {
                    options.AddOpenIddictStore();

                    // Enable Quartz.NET integration.
                    options.UseQuartz();
                })

                // Register the OpenIddict server components.
                .AddServer(options =>
                {
                    options
                        .AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange()
                        .AllowClientCredentialsFlow()
                        .AllowRefreshTokenFlow();

                    options
                        .SetAuthorizationEndpointUris("connect/authorize")
                        .SetLogoutEndpointUris("connect/logout")
                        .SetTokenEndpointUris("connect/token")
                        .SetUserinfoEndpointUris("connect/userinfo");

                    // Encryption and signing of tokens
                    // See https://documentation.openiddict.com/configuration/encryption-and-signing-credentials.html
                    if (env.IsDevelopment())
                    {
                        options.AddEphemeralSigningKey();
                    }
                    else
                    {
                        options.AddSigningCertificate(new X509Certificate2(Convert.FromBase64String(authServerSettings.SigningCertificate)));
                    }

                    if (authServerSettings.EnableAccessTokenEncryption)
                    {
                        options.AddEncryptionKey(new SymmetricSecurityKey(
                             Convert.FromBase64String(authServerSettings.EncryptionSymmetricKey)));
                    }
                    else
                    {
                        options.AddEphemeralEncryptionKey(); // OpenIddict requires an encryption key to be specified in all cases

                        // Disables JWT access token encryption (this option doesn't affect Data Protection tokens).
                        // Disabling encryption is NOT recommended and SHOULD only be done when issuing tokens
                        // to third-party resource servers/APIs you don't control and don't fully trust.
                        options.DisableAccessTokenEncryption();
                    }

                    // Register scopes (permissions)
                    options.RegisterScopes(authServerSettings.Scopes);

                    // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                    // Note that if pass-through is not enabled for some endpoints OpenIddict will handle will handle these requests automatically.
                    options
                        .UseAspNetCore()
                        .EnableAuthorizationEndpointPassthrough()
                        .EnableLogoutEndpointPassthrough()
                        .EnableTokenEndpointPassthrough()
                        .EnableUserinfoEndpointPassthrough()
                        .EnableStatusCodePagesIntegration();
                })

                // Register the OpenIddict validation components.
                .AddValidation(options =>
                {
                    // Import the configuration from the local OpenIddict server instance.
                    options.UseLocalServer();

                    // Register the ASP.NET Core host.
                    options.UseAspNetCore();
                });

            services.AddAuthorization(options => AuthorizationPolicies.Register(options));

            if (env.IsDevelopment())
            {
                services.AddHostedService<OpenIddictConfiguration>(); 
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // make sure to set ASPNETCORE_FORWARDEDHEADERS_ENABLED to true
            // see https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer
            app.UseForwardedHeaders();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSecurityContext();

            app.UseEndpoints(options =>
            {
                options.MapRazorPages();
                options.MapControllers();
            });
        }
    }
}
