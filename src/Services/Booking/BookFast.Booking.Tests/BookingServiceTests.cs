using System.Net;

namespace BookFast.Booking.Tests;

[Collection(nameof(IntegrationTestCollection))]
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
        using var response = await fixture.CreateHttpClient().GetAsync("/");

        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
