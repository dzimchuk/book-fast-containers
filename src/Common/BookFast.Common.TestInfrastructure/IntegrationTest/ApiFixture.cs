using BookFast.Common.Presentation.Authorization;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace BookFast.Common.TestInfrastructure.IntegrationTest
{
    public class ApiFixture<TProgram> : IAsyncLifetime
        where TProgram : class
    {
        protected readonly TestWebApplicationFactory<TProgram> factory;

        //private readonly PostgreSqlContainer dbContainer = new PostgreSqlBuilder()
        //    .WithImage("timescale/timescaledb-ha:pg16")
        //    .WithDatabase("ims")
        //    .WithUsername("postgres")
        //    .WithPassword("postgres")
        //    .WithEnvironment("POSTGRES_INITDB_ARGS", "--encoding=UTF-8 --lc-collate=en_US.utf8")
        //    .Build();

        private readonly MsSqlContainer dbContainer = new MsSqlBuilder().Build();

        public ApiFixture()
        {
            factory = new TestWebApplicationFactory<TProgram>();
        }

        public virtual HttpClient CreateHttpClient(Action<IServiceCollection> configureTestServices = null)
        {
            return factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // override authentication scheme required by authorization policies
                    services.AddAuthorization(options => AuthorizationPolicies.Register(options, Constants.AuthenticationScheme));

                    configureTestServices?.Invoke(services);
                });
            }).CreateClient();
        }

        public IServiceProvider ServiceProvider => factory.Services;

        public virtual async Task InitializeAsync()
        {
            await dbContainer.StartAsync();

            Environment.SetEnvironmentVariable($"ConnectionStrings:IdentitySqlConnection", dbContainer.GetConnectionString());
        }

        public async Task DisposeAsync()
        {
            await dbContainer.StopAsync();
        }
    }
}
