using BookFast.PropertyManagement.Core.Queries.Representations;

namespace BookFast.PropertyManagement.Core.Queries.GetFacility
{
    public class GetFacilityQuery : IRequest<FacilityRepresentation>
    {
        public int Id { get; set; }
    }
}
