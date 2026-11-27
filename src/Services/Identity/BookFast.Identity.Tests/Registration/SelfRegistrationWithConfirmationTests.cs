using System.Net;

namespace BookFast.Identity.Tests.Registration
{
    public class ConfirmationRequiredFixture(IdentityApiFixture fixture) : RegistrationFixture(fixture, requireConfirmedAccount: true);

    public class SelfRegistrationWithConfirmationTests(ConfirmationRequiredFixture fixture) : IClassFixture<ConfirmationRequiredFixture>
    {
        [Fact]
        public async Task MustConfirmEmailBeforeSigningIn()
        {
            const string email = "guest3@registration.test";

            var registration = await fixture.RegisterAsync(email);

            Assert.Equal(HttpStatusCode.Redirect, registration.StatusCode);
            Assert.Contains("RegisterConfirmation", registration.Headers.Location.OriginalString);

            var response = await fixture.LoginAsync(email);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(response.Headers.TryGetValues("Set-Cookie", out var cookies) &&
                         cookies.Any(cookie => cookie.StartsWith(".AspNetCore.Identity.Application=")));
        }
    }
}
