using System;

namespace BookFast.Booking.CommandStack.Validation
{
    public interface IDateRange
    {
        DateTimeOffset FromDate { get; set; }
        DateTimeOffset ToDate { get; set; }
    }
}
