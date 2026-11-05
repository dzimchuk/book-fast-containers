using BookFast.Booking.Domain;

namespace BookFast.Booking.Tests.Domain
{
    public class StayTests
    {
        [Theory]
        [InlineData("2026-08-01", "2026-08-03", 2)]
        [InlineData("2026-08-01", "2026-08-02", 1)]
        public void Nights_ComputesHalfOpenRangeLength(string checkIn, string checkOut, int expectedNights)
        {
            var stay = new Stay(DateOnly.Parse(checkIn), DateOnly.Parse(checkOut));

            Assert.Equal(expectedNights, stay.Nights);
        }

        [Fact]
        public void Overlaps_SharingANight_ReturnsTrue()
        {
            var a = new Stay(DateOnly.Parse("2026-08-01"), DateOnly.Parse("2026-08-05"));
            var b = new Stay(DateOnly.Parse("2026-08-04"), DateOnly.Parse("2026-08-06"));

            Assert.True(a.Overlaps(b));
            Assert.True(b.Overlaps(a));
        }

        [Fact]
        public void Overlaps_SameDayCheckoutCheckin_ReturnsFalse()
        {
            var a = new Stay(DateOnly.Parse("2026-08-01"), DateOnly.Parse("2026-08-03"));
            var b = new Stay(DateOnly.Parse("2026-08-03"), DateOnly.Parse("2026-08-05"));

            Assert.False(a.Overlaps(b));
            Assert.False(b.Overlaps(a));
        }

        [Fact]
        public void Overlaps_DisjointRanges_ReturnsFalse()
        {
            var a = new Stay(DateOnly.Parse("2026-08-01"), DateOnly.Parse("2026-08-02"));
            var b = new Stay(DateOnly.Parse("2026-08-10"), DateOnly.Parse("2026-08-12"));

            Assert.False(a.Overlaps(b));
        }
    }
}
