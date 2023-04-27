using BookFast.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace BookFast.TestInfrastructure.IntegrationTest
{
    internal class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IConfiguration configuration;

        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                               ILoggerFactory logger,
                               UrlEncoder encoder,
                               ISystemClock clock,
                               IConfiguration configuration)
            : base(options, logger, encoder, clock)
        {
            this.configuration = configuration;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new List<Claim>
            {
                new Claim(Claims.Subject, configuration[Constants.UserIdSetting]),
                new Claim(BookFastClaimTypes.TenantId, Constants.CallerTenant),
                new Claim(Claims.Name, Constants.UserName),
                new Claim(Claims.Role, configuration[Constants.RoleSetting])
            };

            var identity = new ClaimsIdentity(claims, "Test",
                nameType: Claims.Name,
                roleType: Claims.Role);

            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Constants.AuthenticationScheme);

            var result = AuthenticateResult.Success(ticket);

            return Task.FromResult(result);
        }
    }
}
