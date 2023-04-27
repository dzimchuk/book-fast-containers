using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.TestInfrastructure.IntegrationTest
{
    public class ApiFixture<TProgram, TDbContext> : IDisposable
        where TProgram : class
        where TDbContext : DbContext
    {
        private static readonly ReaderWriterLockSlim clientLock = new();

        private readonly TDbContext context;
        private bool seeded;

        private readonly TestWebApplicationFactory<TProgram> factory;
        private HttpClient httpClient;

        private bool disposed;

        public ApiFixture()
        {
            factory = new TestWebApplicationFactory<TProgram>();

            var options = new DbContextOptionsBuilder<TDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            context = Activator.CreateInstance(typeof(TDbContext), options) as TDbContext;
        }

        private HttpClient CreateDefaultClient(Action<IServiceCollection> configureServices)
        {
            return factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(context);

                    configureServices?.Invoke(services);
                });
            }).CreateDefaultClient();
        }

        public HttpClient GetHttpClient(Action<IServiceCollection> configureServices = null)
        {
            // Thread synchronization is necessary to overcome ObjectDisposedException when
            // the host tries to initialize LoggerFactory (there is probably some static field involved)
            try
            {
                clientLock.EnterUpgradeableReadLock();
                if (httpClient == null)
                {
                    try
                    {
                        clientLock.EnterWriteLock();
                        if (httpClient == null)
                        {
                            httpClient = CreateDefaultClient(configureServices);
                        }
                    }
                    finally
                    {
                        clientLock.ExitWriteLock();
                    }
                }

                return httpClient;
            }
            finally
            {
                clientLock.ExitUpgradeableReadLock();
            }
        }

        public void UseConfiguration(FixtureConfiguration configuration)
        {
            factory.UseConfiguration(configuration);
        }

        public void Seed(Action<TDbContext> seedAction)
        {
            if (!seeded)
            {
                seedAction(context);
                context.SaveChanges();

                seeded = true;
            }
        }

        public TDbContext DbContext => context;

        public void Dispose()
        {
            factory.Dispose();

            if (!disposed)
            {
                context.Database.EnsureDeleted();
                context.Dispose();

                disposed = true;
            }
        }
    }
}
