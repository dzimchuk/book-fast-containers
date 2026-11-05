using BookFast.Common.Application.Clock;

namespace BookFast.Common.Infrastructure.Clock
{
    internal class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
