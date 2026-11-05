using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using static OpenIddict.Abstractions.OpenIddictConstants;
using ClaimTypes = BookFast.Common.Application.Security.ClaimTypes;

namespace BookFast.Common.TestInfrastructure.IntegrationTest
{
    internal class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly TestSecurityContext testSecurityContext;

        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                               ILoggerFactory logger,
                               UrlEncoder encoder,
                               TestSecurityContext testSecurityContext)
            : base(options, logger, encoder)
        {
            this.testSecurityContext = testSecurityContext;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new List<Claim>
            {
                new Claim(Claims.Subject, testSecurityContext.UserId),
                new Claim(Claims.Name, Constants.UserName),
                new Claim(Claims.Role, testSecurityContext.Role)
            };

            if(!string.IsNullOrEmpty(testSecurityContext.TenantId))
            {
                claims.Add(new Claim(ClaimTypes.TenantId, testSecurityContext.TenantId));
            }

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
