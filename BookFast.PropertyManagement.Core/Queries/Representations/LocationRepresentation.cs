namespace BookFast.PropertyManagement.Core.Queries.Representations
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
