using BookFast.Common.Application.Integration;
using Microsoft.AspNetCore.Authentication;
using BookFast.Identity.Core;
using BookFast.Identity.Core.Models;
using BookFast.Identity.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Text.RegularExpressions;

namespace BookFast.Identity.Tests.Registration
{
    public abstract partial class RegistrationFixture : IAsyncLifetime
    {
        public const string Password = "P@ssw0rd";

        private readonly WebApplicationFactory<Program> factory;
        private readonly IServiceScope scope;
        private readonly IdentityContext dbContext;

        protected RegistrationFixture(IdentityApiFixture fixture, bool? requireConfirmedAccount, bool useIdentityCookie = false)
        {
            factory = fixture.GetWebApplicationFactory(services =>
            {
                services.AddSingleton(Mock.Of<IMailNotificationQueue>());

                // Integration tests normally authenticate every request as a fake caller; pages that act on the signed-in
                // user (account management) need the real Identity cookie instead.
                if (useIdentityCookie)
                {
                    services.Configure<AuthenticationOptions>(options =>
                    {
                        options.DefaultScheme = options.DefaultAuthenticateScheme = options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                    });
                }
            });

            if (requireConfirmedAccount is not null)
            {
                factory = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, config) =>
                    config.AddInMemoryCollection(
                    [
                        new("AuthServer:RequireConfirmedAccount", requireConfirmedAccount.Value.ToString())
                    ])));
            }

            scope = factory.Services.CreateScope();
            dbContext = scope.ServiceProvider.GetRequiredService<IdentityContext>();
        }

        public IDbContext DbContext => dbContext;

        public async Task<HttpResponseMessage> RegisterAsync(string email)
        {
            var client = CreateBrowser();

            return await SubmitFormAsync(client, "/Identity/Account/Register", new Dictionary<string, string>
            {
                ["Input.Email"] = email,
                ["Input.Password"] = Password,
                ["Input.ConfirmPassword"] = Password
            });
        }

        public async Task<HttpResponseMessage> LoginAsync(string email)
        {
            var client = CreateBrowser();

            return await SubmitFormAsync(client, "/Identity/Account/Login", new Dictionary<string, string>
            {
                ["Input.Email"] = email,
                ["Input.Password"] = Password,
                ["Input.RememberMe"] = "false"
            });
        }

        public async Task<HttpResponseMessage> GetPageAsync(string url, HttpClient browser = null) =>
            await (browser ?? CreateBrowser()).GetAsync(url, TestContext.Current.CancellationToken);

        /// <summary>Signs up (confirmation is off unless the fixture says otherwise) and returns the browser holding the sign-in cookie.</summary>
        public async Task<HttpClient> SignUpAndSignInAsync(string email)
        {
            var browser = CreateBrowser();

            var response = await SubmitFormAsync(browser, "/Identity/Account/Register", new Dictionary<string, string>
            {
                ["Input.Email"] = email,
                ["Input.Password"] = Password,
                ["Input.ConfirmPassword"] = Password
            });
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            return browser;
        }

        public Task<HttpResponseMessage> SubmitAsync(HttpClient browser, string url, Dictionary<string, string> form) =>
            SubmitFormAsync(browser, url, form);

        public async Task<string> GetUserIdAsync(string email)
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            return (await userManager.FindByEmailAsync(email)).Id.ToString();
        }

        public async Task<IList<string>> GetRolesAsync(string email)
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var user = await userManager.FindByEmailAsync(email);

            return await userManager.GetRolesAsync(user);
        }

        public ValueTask InitializeAsync() => ValueTask.CompletedTask;

        public async ValueTask DisposeAsync()
        {
            await dbContext.Users.Where(u => u.Email.EndsWith("@registration.test")).ExecuteDeleteAsync();

            scope.Dispose();
            await factory.DisposeAsync();
        }

        public HttpClient CreateBrowser() =>
            factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });

        private static async Task<HttpResponseMessage> SubmitFormAsync(HttpClient client, string url, Dictionary<string, string> form)
        {
            var page = await client.GetAsync(url, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, page.StatusCode);

            var html = await page.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var token = AntiforgeryToken().Match(html).Groups["token"].Value;
            Assert.NotEmpty(token);

            form["__RequestVerificationToken"] = token;

            return await client.PostAsync(url, new FormUrlEncodedContent(form), TestContext.Current.CancellationToken);
        }

        [GeneratedRegex("name=\"__RequestVerificationToken\"[^>]*value=\"(?<token>[^\"]+)\"")]
        private static partial Regex AntiforgeryToken();
    }
}
