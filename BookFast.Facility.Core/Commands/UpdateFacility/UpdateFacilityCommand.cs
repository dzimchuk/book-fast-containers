using MediatR;

namespace BookFast.Facility.Core.Commands.UpdateFacility
{
    public class UpdateFacilityCommand : IRequest
    {
        [SwaggerIgnore]
        public int FacilityId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string StreetAddress { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public string[] Images { get; set; }
    }
}
