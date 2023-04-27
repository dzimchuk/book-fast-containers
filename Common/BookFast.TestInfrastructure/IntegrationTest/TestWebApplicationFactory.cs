using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.TestInfrastructure.IntegrationTest
{
    internal class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
        where TProgram : class
    {
        private FixtureConfiguration fixtureConfiguration;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseConfiguration(GetTestConfiguration());

            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(defaultScheme: Constants.AuthenticationScheme)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        Constants.AuthenticationScheme, options => { });
            });

            builder.UseSetting(Constants.RoleSetting, fixtureConfiguration?.Role ?? Constants.Role);
            builder.UseSetting(Constants.UserIdSetting, fixtureConfiguration?.UserId ?? Constants.UserId);

            static IConfiguration GetTestConfiguration()
            {
                var configurationBuilder = new ConfigurationBuilder();
                configurationBuilder.AddJsonFile("testsettings.json");

                return configurationBuilder.Build();
            }
        }

        public void UseConfiguration(FixtureConfiguration configuration)
        {
            fixtureConfiguration = configuration;
        }
    }
}
