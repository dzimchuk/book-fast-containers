namespace BookFast.Facility.Core.Commands.DeleteFacility
{
    public class DeleteFacilityCommand : IRequest
    {
        public int FacilityId { get; set; }
    }
}
