namespace BookFast.Booking.Tests.Reservations
{
    internal static class StayDates
    {
        private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

        /// <summary>A date <paramref name="days"/> days ahead of today.</summary>
        public static DateOnly Upcoming(int days) => Today.AddDays(days);

        /// <summary>A date <paramref name="days"/> days behind today.</summary>
        public static DateOnly Past(int days) => Today.AddDays(-days);
    }
}
