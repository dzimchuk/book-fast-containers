using BookFast.Common.Application.Security;
using BookFast.Common.TestInfrastructure.IntegrationTest;
using BookFast.PropertyManagement.Domain;
using BookFast.PropertyManagement.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.PropertyManagement.Tests.RentalProperties
{
    public class RentalPropertiesQueryFixture : IAsyncLifetime
    {
        public static readonly Guid Property1Id = new("00000000-0000-0000-0000-000000010001");
        public static readonly Guid Property2Id = new("00000000-0000-0000-0000-000000010002");
        public static readonly Guid Property3Id = new("00000000-0000-0000-0000-000000010003");

        private const string Tenant2Id = "b3c2d1e0-a9b8-4c7d-8e6f-5a4b3c2d1e0a";

        private readonly HttpClient httpClient;
        private readonly IServiceScope scope;
        private readonly PropertyManagementContext dbContext;

        public HttpClient HttpClient => httpClient;

        public RentalPropertiesQueryFixture(PropertyManagementApiFixture fixture)
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
            var property1 = Property.NewProperty(
                Constants.CallerTenant,
                "Mountain Retreat",
                "A cozy mountain retreat",
                new Address("USA", "Colorado", "Denver", "123 Main St", "80203"),
                new Location(39.7392, -104.9903),
                null);
            property1.Id = Property1Id;

            var property2 = Property.NewProperty(
                Constants.CallerTenant,
                "Beach House",
                "Beautiful beachfront property",
                new Address("USA", "California", "Santa Monica", "456 Ocean Ave", "90401"),
                new Location(34.0195, -118.4912),
                null);
            property2.Id = Property2Id;

            var property3 = Property.NewProperty(
                Constants.CallerTenant,
                "City Apartment",
                "Modern city apartment",
                new Address("USA", "New York", "New York", "789 Park Ave", "10021"),
                new Location(40.7128, -74.006),
                null);
            property3.Id = Property3Id;

            var tenant2Property = Property.NewProperty(
                Tenant2Id,
                "Tenant2 Property",
                null,
                new Address("UK", "England", "London", "10 Downing St", "SW1A 2AA"),
                new Location(51.5074, -0.1278),
                null);
            tenant2Property.Id = Guid.CreateVersion7();

            dbContext.Properties.AddRange(property1, property2, property3, tenant2Property);
            await dbContext.SaveChangesAsync();
        }

        public async Task DisposeAsync()
        {
            await dbContext.Properties
                .Where(p => new[] { Property1Id, Property2Id, Property3Id }.Contains(p.Id))
                .ExecuteDeleteAsync();

            await dbContext.Properties
                .Where(p => p.TenantId == Tenant2Id)
                .ExecuteDeleteAsync();

            scope.Dispose();
        }
    }
}
