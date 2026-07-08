using BookFast.Common.Application.Security;
using BookFast.Common.Domain;
using BookFast.Common.TestInfrastructure.IntegrationTest;
using BookFast.PropertyManagement.Domain;
using BookFast.PropertyManagement.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.PropertyManagement.Tests.Accommodations
{
    public class AccommodationsQueryFixture : IAsyncLifetime
    {
        public static readonly Guid PropertyId = new("00000000-0000-0000-0000-000000010011");
        public static readonly Guid Accommodation1Id = new("00000000-0000-0000-0000-000000010021");
        public static readonly Guid Accommodation2Id = new("00000000-0000-0000-0000-000000010022");

        private readonly HttpClient httpClient;
        private readonly IServiceScope scope;
        private readonly PropertyManagementContext dbContext;

        public HttpClient HttpClient => httpClient;

        public AccommodationsQueryFixture(PropertyManagementApiFixture fixture)
        {
            httpClient = fixture.CreateHttpClient(services =>
            {
                services.AddSingleton(new TestSecurityContext
                {
                    UserId = Constants.UserId,
                    Role = Roles.TenantUser,
                    TenantId = Constants.CallerTenant
                });
            });

            scope = fixture.ServiceProvider.CreateScope();
            dbContext = scope.ServiceProvider.GetRequiredService<PropertyManagementContext>();
        }

        public async Task InitializeAsync()
        {
            var property = Property.NewProperty(
                Constants.CallerTenant,
                "Test Property",
                null,
                new Address("USA", "Texas", "Austin", "100 Congress Ave", "78701"),
                new Location(30.2672, -97.7431),
                null);
            property.Id = PropertyId;

            dbContext.Properties.Add(property);
            await dbContext.SaveChangesAsync();

            var accommodation1 = Accommodation.NewAccommodation(Constants.CallerTenant, PropertyId, "Standard Room", null, 1, null, 5, new PriceRange(new Money(100m, "USD"), new Money(100m, "USD")));
            accommodation1.Id = Accommodation1Id;

            var accommodation2 = Accommodation.NewAccommodation(Constants.CallerTenant, PropertyId, "Suite", "Luxury suite with ocean view", 3, null, 2, new PriceRange(new Money(250.5m, "USD"), new Money(250.5m, "USD")));
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
                .Where(p => p.Id == PropertyId)
                .ExecuteDeleteAsync();

            scope.Dispose();
        }
    }
}
