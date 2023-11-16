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

                if (await manager.FindByClientIdAsync("bookfast-client", cancellationToken) is null)
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
                            Permissions.Prefixes.Scope + "client"
                        },
                        Requirements =
                        {
                            Requirements.Features.ProofKeyForCodeExchange
                        }
                    }, cancellationToken);
                }

                if (await manager.FindByClientIdAsync("bookfast-admin", cancellationToken) is null)
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
                            Permissions.Prefixes.Scope + "admin"
                        },
                        Requirements =
                        {
                            Requirements.Features.ProofKeyForCodeExchange
                        }
                    }, cancellationToken);
                }

                if (await manager.FindByClientIdAsync("swagger-ui", cancellationToken) is null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = "swagger-ui",
                        ConsentType = ConsentTypes.Implicit,
                        DisplayName = "Swagger UI client",
                        Type = ClientTypes.Public,
                        PostLogoutRedirectUris =
                        {
                            new Uri("https://localhost:7144"),
                            new Uri("https://localhost:7064"),
                            new Uri("https://localhost:7154"),
                            new Uri("https://localhost:7264")
                        },
                        RedirectUris =
                        {
                            new Uri("https://localhost:7144/swagger/oauth2-redirect.html"),
                            new Uri("https://localhost:7064/swagger/oauth2-redirect.html"),
                            new Uri("https://localhost:7154/swagger/oauth2-redirect.html"),
                            new Uri("https://localhost:7264/swagger/oauth2-redirect.html")
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
                            Permissions.Prefixes.Scope + "identity",
                            Permissions.Prefixes.Scope + "booking",
                            Permissions.Prefixes.Scope + "property-management",
                            Permissions.Prefixes.Scope + "files",
                            Permissions.Prefixes.Scope + "search"
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
                            Permissions.Prefixes.Scope + "admin",
                            Permissions.Prefixes.Scope + "client"
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
                        ClientSecret = "service-client-secret",
                        DisplayName = "Backend service client",
                        Permissions =
                        {
                            Permissions.Endpoints.Token,

                            Permissions.GrantTypes.ClientCredentials,

                            Permissions.Prefixes.Scope + "client",
                            Permissions.Prefixes.Scope + "property-management",
                            Permissions.Prefixes.Scope + "files",

                            Permissions.ResponseTypes.Token
                        }
                    }, cancellationToken);
                }
            }

            static async Task CreateScopesAsync(IServiceScope scope, CancellationToken cancellationToken)
            {
                var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

                if (await manager.FindByNameAsync("admin", cancellationToken) is null)
                {
                    await manager.CreateAsync(new OpenIddictScopeDescriptor
                    {
                        Name = "admin",
                        Resources =
                        {
                            "Identity",
                            "Booking",
                            "PropertyManagement",
                            "Files",
                            "Search"
                        }
                    }, cancellationToken);
                }

                if (await manager.FindByNameAsync("client", cancellationToken) is null)
                {
                    await manager.CreateAsync(new OpenIddictScopeDescriptor
                    {
                        Name = "client",
                        Resources =
                        {
                            "Booking",
                            "Search"
                        }
                    }, cancellationToken);
                }

                if (await manager.FindByNameAsync("identity", cancellationToken) is null)
                {
                    await manager.CreateAsync(new OpenIddictScopeDescriptor
                    {
                        Name = "identity",
                        Resources =
                        {
                            "Identity"
                        }
                    }, cancellationToken);
                }

                if (await manager.FindByNameAsync("booking", cancellationToken) is null)
                {
                    await manager.CreateAsync(new OpenIddictScopeDescriptor
                    {
                        Name = "booking",
                        Resources =
                        {
                            "Booking"
                        }
                    }, cancellationToken);
                }

                if (await manager.FindByNameAsync("property-management", cancellationToken) is null)
                {
                    await manager.CreateAsync(new OpenIddictScopeDescriptor
                    {
                        Name = "property-management",
                        Resources =
                        {
                            "PropertyManagement"
                        }
                    }, cancellationToken);
                }

                if (await manager.FindByNameAsync("files", cancellationToken) is null)
                {
                    await manager.CreateAsync(new OpenIddictScopeDescriptor
                    {
                        Name = "files",
                        Resources =
                        {
                            "Files"
                        }
                    }, cancellationToken);
                }

                if (await manager.FindByNameAsync("search", cancellationToken) is null)
                {
                    await manager.CreateAsync(new OpenIddictScopeDescriptor
                    {
                        Name = "search",
                        Resources =
                        {
                            "Search"
                        }
                    }, cancellationToken);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
