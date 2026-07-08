using System.Text.Json.Serialization;

namespace BookFast.PropertyManagement.Domain
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Facility
    {
        WiFi,
        AirConditioning,
        Parking,
        Pool,
        Restaurant,
        Gym,
        Refrigerator,
        Kitchenette
    }
}
