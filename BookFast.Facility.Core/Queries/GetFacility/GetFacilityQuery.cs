using BookFast.Facility.Core.Queries.Representations;

namespace BookFast.Facility.Core.Queries.GetFacility
{
    public class GetFacilityQuery : IRequest<FacilityRepresentation>
    {
        public int Id { get; set; }
    }
}
