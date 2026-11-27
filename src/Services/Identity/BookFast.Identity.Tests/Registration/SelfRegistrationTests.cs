using BookFast.Common.Application.Security;
using System.Net;

namespace BookFast.Identity.Tests.Registration
{
    // Account confirmation is turned off for automated tests (see IdentityApiFixture)
    public class SelfRegistrationFixture(IdentityApiFixture fixture) : RegistrationFixture(fixture, requireConfirmedAccount: null);

    public class SelfRegistrationTests(SelfRegistrationFixture fixture) : IClassFixture<SelfRegistrationFixture>
    {
        [Fact]
        public async Task NewAccountIsCustomer()
        {
            const string email = "guest1@registration.test";

            var response = await fixture.RegisterAsync(email);

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            Assert.Equal([Roles.Customer], await fixture.GetRolesAsync(email));
        }

        [Fact]
        public async Task CanSignInWithoutConfirmingEmail()
        {
            const string email = "guest2@registration.test";

            await fixture.RegisterAsync(email);
            var response = await fixture.LoginAsync(email);

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains(response.Headers.GetValues("Set-Cookie"), cookie => cookie.StartsWith(".AspNetCore.Identity.Application="));
        }
    }
}
