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
            using var scope = serviceProvider.CreateScope();

            await CreateApplicationAsync(scope, cancellationToken);
            await CreateScopesAsync(scope, cancellationToken);

            static async Task CreateApplicationAsync(IServiceScope scope, CancellationToken cancellationToken)
            {
                var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

                if (await manager.FindByClientIdAsync("bookfast-client") is null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = "bookfast-client",
                        ConsentType = ConsentTypes.Implicit,
                        DisplayName = "BookFast client application",
                        Type = ClientTypes.Public,
                        PostLogoutRedirectUris =
                        {
                            new Uri("https://localhost:7288/authentication/logout-callback")
                        },
                        RedirectUris =
                        {
                            new Uri("https://localhost:7288/authentication/login-callback")
                        },
                        Permissions =
                        {
                            Permissions.Endpoints.Authorization,
                            Permissions.Endpoints.Logout,
                            Permissions.Endpoints.Token,

                            Permissions.GrantTypes.AuthorizationCode,
                            Permissions.GrantTypes.RefreshToken,

                            Permissions.ResponseTypes.Code,

                            Permissions.Scopes.Email,
                            Permissions.Scopes.Profile,
                            Permissions.Scopes.Roles,
                            Permissions.Prefixes.Scope + "booking"
                        },
                        Requirements =
                        {
                            Requirements.Features.ProofKeyForCodeExchange
                        }
                    }, cancellationToken);
                }

                if (await manager.FindByClientIdAsync("bookfast-admin") is null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = "bookfast-admin",
                        ConsentType = ConsentTypes.Implicit,
                        DisplayName = "BookFast admin application",
                        Type = ClientTypes.Public,
                        PostLogoutRedirectUris =
                        {
                            new Uri("https://localhost:7200/authentication/logout-callback")
                        },
                        RedirectUris =
                        {
                            new Uri("https://localhost:7200/authentication/login-callback")
                        },
                        Permissions =
                        {
                            Permissions.Endpoints.Authorization,
                            Permissions.Endpoints.Logout,
                            Permissions.Endpoints.Token,

                            Permissions.GrantTypes.AuthorizationCode,
                            Permissions.GrantTypes.RefreshToken,

                            Permissions.ResponseTypes.Code,

                            Permissions.Scopes.Email,
                            Permissions.Scopes.Profile,
                            Permissions.Scopes.Roles,
                            Permissions.Prefixes.Scope + "all"
                        },
                        Requirements =
                        {
                            Requirements.Features.ProofKeyForCodeExchange
                        }
                    }, cancellationToken);
                }

                if (await manager.FindByClientIdAsync("postman") is null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = "postman",
                        ConsentType = ConsentTypes.Implicit,
                        DisplayName = "Postman",
                        Type = ClientTypes.Public,
                        RedirectUris =
                        {
                            new Uri("https://oauth.pstmn.io/v1/callback")
                        },
                        Permissions =
                        {
                            Permissions.Endpoints.Authorization,
                            Permissions.Endpoints.Logout,
                            Permissions.Endpoints.Token,

                            Permissions.GrantTypes.AuthorizationCode,
                            Permissions.GrantTypes.RefreshToken,

                            Permissions.ResponseTypes.Code,

                            Permissions.Scopes.Email,
                            Permissions.Scopes.Profile,
                            Permissions.Scopes.Roles,
                            Permissions.Prefixes.Scope + "all",
                            Permissions.Prefixes.Scope + "booking",
                            Permissions.Prefixes.Scope + "facility"
                        },
                        Requirements =
                        {
                            Requirements.Features.ProofKeyForCodeExchange
                        }
                    }, cancellationToken);
                }

                if (await manager.FindByClientIdAsync("service-client", cancellationToken) is null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = "service-client",
                        ClientSecret = "service-client-secret", // TODO: generate proper secret
                        DisplayName = "Backend service client",
                        Permissions =
                        {
                            Permissions.Endpoints.Token,

                            Permissions.GrantTypes.ClientCredentials,

                            Permissions.Prefixes.Scope + "booking",
                            Permissions.Prefixes.Scope + "facility",

                            Permissions.ResponseTypes.Token
                        }
                    }, cancellationToken);
                }
            }

            static async Task CreateScopesAsync(IServiceScope scope, CancellationToken cancellationToken)
            {
                var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

                if (await manager.FindByNameAsync("all") is null)
                {
                    await manager.CreateAsync(new OpenIddictScopeDescriptor
                    {
                        Name = "all",
                        Resources =
                        {
                            "booking-api",
                            "facility-api"
                        }
                    }, cancellationToken);
                }

                if (await manager.FindByNameAsync("booking") is null)
                {
                    await manager.CreateAsync(new OpenIddictScopeDescriptor
                    {
                        Name = "booking",
                        Resources =
                        {
                            "booking-api"
                        }
                    }, cancellationToken);
                }

                if (await manager.FindByNameAsync("facility") is null)
                {
                    await manager.CreateAsync(new OpenIddictScopeDescriptor
                    {
                        Name = "facility",
                        Resources =
                        {
                            "facility-api"
                        }
                    }, cancellationToken);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
