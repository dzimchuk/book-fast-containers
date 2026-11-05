using System.Text.Json.Serialization;

namespace BookFast.Booking.Domain
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ReservationStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Expired,
        Completed
    }
}
