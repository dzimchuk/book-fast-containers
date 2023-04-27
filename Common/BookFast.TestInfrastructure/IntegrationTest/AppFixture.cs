using BookFast.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using System.Security.Claims;

namespace BookFast.TestInfrastructure.IntegrationTest
{
    public class AppFixture<TDbContext> : IDisposable
        where TDbContext : DbContext
    {
        private readonly TDbContext context;
        private bool seeded;

        public AppFixture()
        {
            var options = new DbContextOptionsBuilder<TDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            context = Activator.CreateInstance(typeof(TDbContext), options) as TDbContext;
        }

        public IMediator InitializeApp(Action<IServiceCollection, IConfiguration> configureServices = null)
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("testsettings.json");

            var configuration = configurationBuilder.Build();

            var services = new ServiceCollection();

            services.AddSingleton(context);

            configureServices?.Invoke(services, configuration);

            var serviceProvider = services.BuildServiceProvider();

            var identity = new ClaimsIdentity();
            identity.SetClaim(BookFastClaimTypes.TenantId, Constants.CallerTenant);

            var securityContext = serviceProvider.GetRequiredService<ISecurityContext>() as ISecurityContextAcceptor;
            securityContext.Principal = new ClaimsPrincipal(identity);

            return serviceProvider.GetRequiredService<IMediator>();
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
            context.Database.EnsureDeleted();
            context.Dispose();
        }
    }
}
