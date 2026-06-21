using BookFast.PropertyManagement.Domain;

namespace BookFast.PropertyManagement.Application.RentalProperties
{
    public class LocationRepresentation
    {
        /// <summary>
        /// Longitude
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// Latitude
        /// </summary>
        public double? Latitude { get; set; }

        public static LocationRepresentation Map(Location location) =>
            new()
            {
                Longitude = location.Longitude,
                Latitude = location.Latitude
            };
    }
}
