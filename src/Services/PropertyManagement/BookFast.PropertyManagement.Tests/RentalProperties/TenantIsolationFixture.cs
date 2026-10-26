using BookFast.Common.Application.Security;
using BookFast.Common.TestInfrastructure.IntegrationTest;
using BookFast.PropertyManagement.Domain;
using BookFast.PropertyManagement.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.PropertyManagement.Tests.RentalProperties
{
    public class TenantIsolationFixture : IAsyncLifetime
    {
        public const string OtherTenantId = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee";

        private readonly WebApplicationFactory<Program> factory;
        private readonly HttpClient httpClient;
        private readonly IServiceScope scope;
        private readonly PropertyManagementContext dbContext;

        public HttpClient HttpClient => httpClient;

        public Guid PropertyId { get; private set; }

        public TenantIsolationFixture(PropertyManagementApiFixture fixture)
        {
            factory = fixture.GetWebApplicationFactory(services =>
            {
                services.AddSingleton(new TestSecurityContext
                {
                    UserId = Constants.UserId,
                    Role = Roles.TenantUser,
                    TenantId = OtherTenantId
                });
            });
            httpClient = factory.CreateClient();

            scope = factory.Services.CreateScope();
            dbContext = scope.ServiceProvider.GetRequiredService<PropertyManagementContext>();
        }

        public async Task InitializeAsync()
        {
            var property = Property.NewProperty(
                Constants.CallerTenant,
                "Tenant1 Property",
                "Belongs to CallerTenant",
                new Address("USA", "Texas", "Austin", "100 Congress Ave", "78701"),
                new Location(30.2672, -97.7431),
                null,
                null);

            property.Id = Guid.CreateVersion7();

            dbContext.Properties.Add(property);
            await dbContext.SaveChangesAsync();

            PropertyId = property.Id;
        }

        public async Task DisposeAsync()
        {
            await dbContext.Properties
                .Where(p => p.Id == PropertyId)
                .ExecuteDeleteAsync();

            scope.Dispose();
            await factory.DisposeAsync();
        }
    }
}
