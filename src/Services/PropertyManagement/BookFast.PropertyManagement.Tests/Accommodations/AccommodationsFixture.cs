using BookFast.Common.Application.Security;
using BookFast.Common.TestInfrastructure.IntegrationTest;
using BookFast.PropertyManagement.Application;
using BookFast.PropertyManagement.Domain;
using BookFast.PropertyManagement.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.PropertyManagement.Tests.Accommodations
{
    public class AccommodationsFixture : IAsyncLifetime
    {
        private readonly HttpClient httpClient;
        private readonly IServiceScope scope;
        private readonly PropertyManagementContext dbContext;

        public HttpClient HttpClient => httpClient;

        public IDbContext DbContext => dbContext;

        public int PropertyId { get; private set; }
        public int AccommodationId { get; private set; }

        public AccommodationsFixture(ApiFixture<Program> fixture)
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
                "A property for accommodation tests",
                new Address("USA", "Texas", "Austin", "100 Congress Ave", "78701"),
                new Location(30.2672, -97.7431),
                null);

            dbContext.Properties.Add(property);
            await dbContext.SaveChangesAsync();

            PropertyId = property.Id;

            var accommodation = Accommodation.NewAccommodation(Constants.CallerTenant, PropertyId, "Standard Room", null, 1, null, 5, 100m);
            dbContext.Accommodations.Add(accommodation);
            await dbContext.SaveChangesAsync();

            AccommodationId = accommodation.Id;
        }

        public async Task DisposeAsync()
        {
            await dbContext.Accommodations
                .Where(a => a.PropertyId == PropertyId)
                .ExecuteDeleteAsync();

            await dbContext.Properties
                .Where(p => p.Id == PropertyId)
                .ExecuteDeleteAsync();

            scope.Dispose();
        }
    }
}
