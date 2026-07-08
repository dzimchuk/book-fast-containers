namespace BookFast.PropertyManagement.Domain
{
    public static class FacilityScopes
    {
        private static readonly Dictionary<Facility, FacilityScope> scopes = new()
        {
            [Facility.WiFi] = FacilityScope.Both,
            [Facility.AirConditioning] = FacilityScope.Both,
            [Facility.Parking] = FacilityScope.Property,
            [Facility.Pool] = FacilityScope.Property,
            [Facility.Restaurant] = FacilityScope.Property,
            [Facility.Gym] = FacilityScope.Property,
            [Facility.Refrigerator] = FacilityScope.Accommodation,
            [Facility.Kitchenette] = FacilityScope.Accommodation
        };

        public static FacilityScope GetScope(this Facility facility) => scopes[facility];

        public static bool AppliesToProperty(this Facility facility) =>
            GetScope(facility) is FacilityScope.Property or FacilityScope.Both;

        public static bool AppliesToAccommodation(this Facility facility) =>
            GetScope(facility) is FacilityScope.Accommodation or FacilityScope.Both;
    }
}
