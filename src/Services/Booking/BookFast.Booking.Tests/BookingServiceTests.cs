using System.Net;

namespace BookFast.Booking.Tests;

public sealed class BookingServiceTests
{
    private readonly BookingApiFixture fixture;

    public BookingServiceTests(BookingApiFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task Host_starts_successfully()
    {
        using var factory = fixture.GetWebApplicationFactory();
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/", TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
