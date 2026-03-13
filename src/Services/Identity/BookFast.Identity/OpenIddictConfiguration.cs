using BookFast.Identity.Infrastructure;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace BookFast.Identity
{
    internal class OpenIddictConfiguration : IHostedService
    {
        private readonly IServiceProvider serviceProvider;

        public OpenIddictConfiguration(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            ApplyMigrations();

            using var scope = serviceProvider.CreateScope();

            await CreateApplicationAsync(scope, cancellationToken);
            await CreateScopesAsync(scope, cancellationToken);

            void ApplyMigrations()
            {
                using var scope = serviceProvider.CreateScope();
                MigrationExtensions.ApplyMigration(scope);
            }

            static async Task CreateApplicationAsync(IServiceScope scope, CancellationToken cancellationToken)
            {
                var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

                if (await manager.FindByClientIdAsync("swagger-ui", cancellationToken) is null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = "swagger-ui",
                        ConsentType = ConsentTypes.Implicit,
                        DisplayName = "Swagger UI client",
                        ClientType = ClientTypes.Public,
                        PostLogoutRedirectUris =
                        {
                            new Uri("https://localhost:5001"), // Identity
                            new Uri("https://localhost:5002"), // PropertyManagement
                            new Uri("https://localhost:5003"), // Search
                            new Uri("https://localhost:5004"), // Booking
                        },
                        RedirectUris =
                        {
                            new Uri("https://localhost:5001/swagger/oauth2-redirect.html"),
                            new Uri("https://localhost:5002/swagger/oauth2-redirect.html"),
                            new Uri("https://localhost:5003/swagger/oauth2-redirect.html"),
                            new Uri("https://localhost:5004/swagger/oauth2-redirect.html")
                        },
                        Permissions =
                        {
                            Permissions.Endpoints.Authorization,
                            Permissions.Endpoints.EndSession,
                            Permissions.Endpoints.Token,

                            Permissions.GrantTypes.AuthorizationCode,
                            Permissions.GrantTypes.RefreshToken,

                            Permissions.ResponseTypes.Code,

                            Permissions.Scopes.Email,
                            Permissions.Scopes.Profile,
                            Permissions.Scopes.Roles,
                            Permissions.Prefixes.Scope + "provider",
                            Permissions.Prefixes.Scope + "customer"
                        },
                        Requirements =
                        {
                            Requirements.Features.ProofKeyForCodeExchange
                        }
                    });
                }

                if (await manager.FindByClientIdAsync("postman", cancellationToken) is null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = "postman",
                        ConsentType = ConsentTypes.Implicit,
                        DisplayName = "Postman",
                        ClientType = ClientTypes.Public,
                        RedirectUris =
                        {
                            new Uri("https://oauth.pstmn.io/v1/callback")
                        },
                        Permissions =
                        {
                            Permissions.Endpoints.Authorization,
                            Permissions.Endpoints.EndSession,
                            Permissions.Endpoints.Token,

                            Permissions.GrantTypes.AuthorizationCode,
                            Permissions.GrantTypes.RefreshToken,

                            Permissions.ResponseTypes.Code,

                            Permissions.Scopes.Email,
                            Permissions.Scopes.Profile,
                            Permissions.Scopes.Roles,
                            Permissions.Prefixes.Scope + "provider",
                            Permissions.Prefixes.Scope + "customer"
                        },
                        Requirements =
                        {
                            Requirements.Features.ProofKeyForCodeExchange
                        }
                    }, cancellationToken);
                }

                //if (await manager.FindByClientIdAsync("service-client", cancellationToken) is null)
                //{
                //    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                //    {
                //        ClientId = "service-client",
                //        ClientSecret = "service-client-secret",
                //        DisplayName = "Backend service client",
                //        Permissions =
                //        {
                //            Permissions.Endpoints.Token,

                //            Permissions.GrantTypes.ClientCredentials,

                //            Permissions.Prefixes.Scope + "provider",

                //            Permissions.ResponseTypes.Token
                //        }
                //    }, cancellationToken);
                //}
            }

            static async Task CreateScopesAsync(IServiceScope scope, CancellationToken cancellationToken)
            {
                var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

                if (await manager.FindByNameAsync("provider", cancellationToken) is null)
                {
                    await manager.CreateAsync(new OpenIddictScopeDescriptor
                    {
                        Name = "provider",
                        Resources =
                        {
                            "Identity",
                            "PropertyManagement",
                            "Search"
                        }
                    }, cancellationToken);
                }

                if (await manager.FindByNameAsync("customer", cancellationToken) is null)
                {
                    await manager.CreateAsync(new OpenIddictScopeDescriptor
                    {
                        Name = "customer",
                        Resources =
                        {
                            "Booking",
                            "Search"
                        }
                    }, cancellationToken);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
