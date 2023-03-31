using BookFast.Api.Cors;
using BookFast.Identity.Core.Models;
using BookFast.Identity.Infrastructure;
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
            services.AddIdentityDbContext(configuration);

            services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<Role>()
                .AddIdentityStore();

            services.AddRazorPages();
            services.AddControllers();

            services.AddCorsServices(configuration);

            // OpenIddict offers native integration with Quartz.NET to perform scheduled tasks
            // (like pruning orphaned authorizations/tokens from the database) at regular intervals.
            services.AddQuartz(options =>
            {
                options.UseMicrosoftDependencyInjectionJobFactory();
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
                        options.AddSigningCertificate(DevSigningCertificate);
                    }
                    else
                    {
                        options.AddSigningCertificate(authServerSettings.SigningCertificateThumbprint, StoreName.My, StoreLocation.CurrentUser);
                    }

                    if (authServerSettings.EnableAccessTokenEncryption)
                    {
                        options.AddEncryptionKey(new SymmetricSecurityKey(
                             Convert.FromBase64String(authServerSettings.EncryptionSymmetricKey)));
                    }
                    else
                    {
                        options.AddEphemeralEncryptionKey();

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

            app.UseEndpoints(options =>
            {
                options.MapRazorPages();
                options.MapControllers();
            });
        }

        private static X509Certificate2 DevSigningCertificate
        {
            get
            {
                using var stream = typeof(Startup).Assembly.GetManifestResourceStream("BookFast.Identity.certs.signing-certificate.pfx");
                using var memoryStream = new MemoryStream();

                stream.CopyTo(memoryStream);

                return new X509Certificate2(memoryStream.ToArray());
            }
        }
    }
}
