using MediatR;
using System.ComponentModel.DataAnnotations;

namespace BookFast.Facility.Core.Commands.CreateFacility
{
    public class CreateFacilityCommand : IRequest<int>
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string StreetAddress { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public string[] Images { get; set; }
    }
}
