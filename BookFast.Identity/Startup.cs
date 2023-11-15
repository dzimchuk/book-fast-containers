using BookFast.Api;
using BookFast.Api.Cors;
using BookFast.Api.SecurityContext;
using BookFast.Identity.Core;
using BookFast.Identity.Core.Models;
using BookFast.Identity.Infrastructure;
using BookFast.Identity.Services;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using System.Security.Cryptography.X509Certificates;
using static OpenIddict.Abstractions.OpenIddictConstants;

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

            services.AddDefaultIdentity<User>(options =>
                {
                    // TODO: Place to configure Identity (password rules, registration confirmation, MFA and so on)
                    options.SignIn.RequireConfirmedAccount = true;

                    if (env.IsDevelopment()) // for testing purposes
                    {
                        options.SignIn.RequireConfirmedAccount = false;
                    }
                })
                .AddRoles<Role>()
                .AddIdentityStore();

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
                    var authServerSettings = configuration.GetAuthServerSettings();

                    options
                        .AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange()
                        //.AllowClientCredentialsFlow()
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
                    options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles, "all", "booking", "facility");

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

            services.AddHostedService<OpenIddictConfiguration>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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
