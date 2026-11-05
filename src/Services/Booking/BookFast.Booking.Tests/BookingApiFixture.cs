using BookFast.Booking.Integration;
using BookFast.Common.TestInfrastructure.IntegrationTest;

namespace BookFast.Booking.Tests;

public sealed class BookingApiFixture : ApiFixture<Program>
{
    public IntegrationEventHarness IntegrationEvents { get; } = new IntegrationEventHarness()
        .Observe<AccommodationBookableChanged>();

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        Environment.SetEnvironmentVariable("Outbox:QueryDelay", "00:00:01");
        Environment.SetEnvironmentVariable("Payments:SettlementDelay", "00:00:00");

        await IntegrationEvents.StartAsync();
    }

    public override async Task DisposeAsync()
    {
        await IntegrationEvents.StopAsync();

        await base.DisposeAsync();
    }
}
