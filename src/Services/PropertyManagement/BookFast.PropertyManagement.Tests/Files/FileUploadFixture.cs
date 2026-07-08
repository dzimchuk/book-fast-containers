using BookFast.Common.Application.Security;
using BookFast.Common.Domain;
using BookFast.Common.TestInfrastructure.IntegrationTest;
using BookFast.PropertyManagement.Domain;
using BookFast.PropertyManagement.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.PropertyManagement.Tests.Files
{
    public class FileUploadFixture : IAsyncLifetime
    {
        private readonly HttpClient httpClient;
        private readonly IServiceScope scope;
        private readonly PropertyManagementContext dbContext;

        public HttpClient HttpClient => httpClient;

        public Guid PropertyId { get; private set; }
        public Guid AccommodationId { get; private set; }

        public FileUploadFixture(PropertyManagementApiFixture fixture)
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
                "Upload Test Property",
                "A property for file upload tests",
                new Address("USA", "Texas", "Austin", "100 Congress Ave", "78701"),
                new Location(30.2672, -97.7431),
                null,
                null);
            property.Id = Guid.CreateVersion7();

            dbContext.Properties.Add(property);
            await dbContext.SaveChangesAsync();

            PropertyId = property.Id;

            var accommodation = Accommodation.NewAccommodation(
                Constants.CallerTenant,
                PropertyId,
                "Upload Test Room",
                null,
                1,
                null,
                5,
                new PriceRange(new Money(100m, "USD"), new Money(100m, "USD")),
                null);
            accommodation.Id = Guid.CreateVersion7();

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
