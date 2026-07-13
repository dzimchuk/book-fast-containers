using BookFast.Common.Application.Security;
using BookFast.Common.TestInfrastructure.IntegrationTest;
using BookFast.PropertyManagement.Domain;
using BookFast.PropertyManagement.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.PropertyManagement.Tests.Replay
{
    public class ReplayConcurrentTrafficFixture : IAsyncLifetime
    {
        public const string TenantId = "e5f6a7b8-c9d0-4e1f-8a2b-3c4d5e6f7a8b";

        public static readonly Guid ExistingPropertyId = new("00000000-0000-0000-0000-000000030001");

        private readonly HttpClient httpClient;
        private readonly IServiceScope scope;
        private readonly PropertyManagementContext dbContext;

        public HttpClient HttpClient => httpClient;
        public IntegrationEventHarness IntegrationEvents { get; }

        public ReplayConcurrentTrafficFixture(PropertyManagementApiFixture fixture)
        {
            IntegrationEvents = fixture.IntegrationEvents;

            httpClient = fixture.CreateHttpClient(services =>
            {
                services.AddSingleton(new TestSecurityContext
                {
                    UserId = Constants.UserId,
                    Role = Roles.TenantAdmin,
                    TenantId = TenantId
                });
            });

            scope = fixture.ServiceProvider.CreateScope();
            dbContext = scope.ServiceProvider.GetRequiredService<PropertyManagementContext>();
        }

        public async Task InitializeAsync()
        {
            var property = Property.NewProperty(
                TenantId,
                "Pre-existing Property",
                "Replayed while concurrent traffic occurs",
                new Address("USA", "Texas", "Austin", "100 Congress Ave", "78701"),
                new Location(30.2672, -97.7431),
                null,
                null);
            property.Id = ExistingPropertyId;

            dbContext.Properties.Add(property);
            await dbContext.SaveChangesAsync();
        }

        public async Task DisposeAsync()
        {
            await dbContext.Properties
                .Where(p => p.Id == ExistingPropertyId)
                .ExecuteDeleteAsync();

            scope.Dispose();
        }
    }
}
