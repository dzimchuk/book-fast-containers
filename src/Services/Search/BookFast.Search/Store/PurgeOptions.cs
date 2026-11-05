namespace BookFast.Search.Store
{
    internal class PurgeOptions
    {
        public TimeSpan Window { get; set; } = TimeSpan.FromDays(1);

        public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(15);
    }
}
