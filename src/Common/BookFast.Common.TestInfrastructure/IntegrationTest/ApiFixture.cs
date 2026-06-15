using BookFast.Common.Presentation.Authorization;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

namespace BookFast.Common.TestInfrastructure.IntegrationTest
{
    public class ApiFixture<TProgram> : IAsyncLifetime
        where TProgram : class
    {
        protected readonly TestWebApplicationFactory<TProgram> factory;

        private readonly MsSqlContainer dbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("P@ssw0rd")
            .WithEnvironment("MSSQL_PID", "Developer")
            .Build();

        private readonly RabbitMqContainer rabbitMqContainer = new RabbitMqBuilder("rabbitmq:management-alpine")
            .WithUsername("guest")
            .WithPassword("guest")
            .Build();

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
            await rabbitMqContainer.StartAsync();

            Environment.SetEnvironmentVariable($"ConnectionStrings:Sql", dbContainer.GetConnectionString());
            Environment.SetEnvironmentVariable($"ConnectionStrings:MessageBus", rabbitMqContainer.GetConnectionString());
        }

        public virtual async Task DisposeAsync()
        {
            await dbContainer.StopAsync();
            await rabbitMqContainer.StopAsync();
        }
    }
}
