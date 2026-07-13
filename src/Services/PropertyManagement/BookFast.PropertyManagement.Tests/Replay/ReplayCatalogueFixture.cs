using BookFast.Common.Application.Security;
using BookFast.Common.Domain;
using BookFast.Common.TestInfrastructure.IntegrationTest;
using BookFast.PropertyManagement.Domain;
using BookFast.PropertyManagement.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.PropertyManagement.Tests.Replay
{
    public class ReplayCatalogueFixture : IAsyncLifetime
    {
        public const string TenantId = "d2e3f4a5-b6c7-4d8e-9f0a-1b2c3d4e5f6a";

        public static readonly Guid Property1Id = new("00000000-0000-0000-0000-000000020001");
        public static readonly Guid Property2Id = new("00000000-0000-0000-0000-000000020002");
        public static readonly Guid Accommodation1Id = new("00000000-0000-0000-0000-000000020011");
        public static readonly Guid Accommodation2Id = new("00000000-0000-0000-0000-000000020012");

        private readonly HttpClient httpClient;
        private readonly IServiceScope scope;
        private readonly PropertyManagementContext dbContext;

        public HttpClient HttpClient => httpClient;
        public IntegrationEventHarness IntegrationEvents { get; }

        public ReplayCatalogueFixture(PropertyManagementApiFixture fixture)
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
            var property1 = Property.NewProperty(
                TenantId,
                "Replay Property 1",
                "First property to be replayed",
                new Address("USA", "Texas", "Austin", "100 Congress Ave", "78701"),
                new Location(30.2672, -97.7431),
                null,
                null);
            property1.Id = Property1Id;

            var property2 = Property.NewProperty(
                TenantId,
                "Replay Property 2",
                "Second property to be replayed",
                new Address("USA", "Colorado", "Denver", "123 Main St", "80203"),
                new Location(39.7392, -104.9903),
                null,
                null);
            property2.Id = Property2Id;

            dbContext.Properties.AddRange(property1, property2);
            await dbContext.SaveChangesAsync();

            var accommodation1 = Accommodation.NewAccommodation(
                TenantId, Property1Id, "Standard Room", null, 1, null, 5,
                new PriceRange(new Money(100m, "USD"), new Money(100m, "USD")), [Facility.WiFi]);
            accommodation1.Id = Accommodation1Id;

            var accommodation2 = Accommodation.NewAccommodation(
                TenantId, Property1Id, "Suite", "Luxury suite", 3, null, 2,
                new PriceRange(new Money(250m, "USD"), new Money(250m, "USD")), [Facility.WiFi]);
            accommodation2.Id = Accommodation2Id;

            dbContext.Accommodations.AddRange(accommodation1, accommodation2);
            await dbContext.SaveChangesAsync();
        }

        public async Task DisposeAsync()
        {
            await dbContext.Accommodations
                .Where(a => new[] { Accommodation1Id, Accommodation2Id }.Contains(a.Id))
                .ExecuteDeleteAsync();

            await dbContext.Properties
                .Where(p => new[] { Property1Id, Property2Id }.Contains(p.Id))
                .ExecuteDeleteAsync();

            scope.Dispose();
        }
    }
}
