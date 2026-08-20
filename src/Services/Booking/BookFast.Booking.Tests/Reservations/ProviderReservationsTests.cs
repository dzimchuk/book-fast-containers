using BookFast.Booking.Domain;
using BookFast.Booking.Infrastructure.Database;
using BookFast.Common.Application.Security;
using BookFast.Common.Domain;
using BookFast.Common.TestInfrastructure;
using BookFast.Common.TestInfrastructure.IntegrationTest;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;

namespace BookFast.Booking.Tests.Reservations
{
    [Collection(nameof(IntegrationTestCollection))]
    public class ProviderReservationsTests : IAsyncLifetime
    {
        private const string TenantId = "tenant-1";
        private const string OtherTenantId = "tenant-2";

        private readonly WebApplicationFactory<Program> factory;
        private readonly IServiceScope scope;
        private readonly BookingContext dbContext;

        public HttpClient HttpClient { get; }

        public ProviderReservationsTests(BookingApiFixture fixture)
        {
            factory = fixture.GetWebApplicationFactory(services =>
            {
                services.AddSingleton(new TestSecurityContext
                {
                    UserId = Constants.UserId,
                    Role = Roles.TenantUser,
                    TenantId = TenantId
                });
            });

            HttpClient = factory.CreateClient();
            scope = factory.Services.CreateScope();
            dbContext = scope.ServiceProvider.GetRequiredService<BookingContext>();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            scope.Dispose();
            await factory.DisposeAsync();
        }

        private async Task<Guid> SeedAccommodationAsync(string tenantId)
        {
            var accommodation = Accommodation.NewAccommodation(Guid.NewGuid(), tenantId, Guid.NewGuid());
            accommodation.ApplyCreated(5, new Money(100m, "USD"), DateTimeOffset.UtcNow);

            dbContext.Accommodations.Add(accommodation);
            await dbContext.SaveChangesAsync();

            return accommodation.Id;
        }

        private async Task<Guid> SeedReservationAsync(Guid accommodationId, string tenantId, DateOnly checkIn, DateOnly checkOut)
        {
            var reservation = Reservation.NewReservation(
                Guid.NewGuid(), Constants.UserId, accommodationId, tenantId, new Stay(checkIn, checkOut), 1, new Money(100m, "USD"),
                DateTimeOffset.UtcNow.AddMinutes(15));

            dbContext.Reservations.Add(reservation);
            await dbContext.SaveChangesAsync();

            return reservation.Id;
        }

        [Fact]
        public async Task List_ReturnsOnlyMyTenantsReservations()
        {
            var myAccommodationId = await SeedAccommodationAsync(TenantId);
            var otherAccommodationId = await SeedAccommodationAsync(OtherTenantId);

            var mineId = await SeedReservationAsync(
                myAccommodationId, TenantId, DateOnly.Parse("2027-01-01"), DateOnly.Parse("2027-01-03"));
            var othersId = await SeedReservationAsync(
                otherAccommodationId, OtherTenantId, DateOnly.Parse("2027-02-01"), DateOnly.Parse("2027-02-03"));

            var reservations = await HttpClient.GetFromJsonAsync<JsonElement[]>("/api/reservations/provider");

            Assert.Contains(reservations, r => r.GetProperty("id").GetGuid() == mineId);
            Assert.DoesNotContain(reservations, r => r.GetProperty("id").GetGuid() == othersId);
        }
    }
}
