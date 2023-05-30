namespace BookFast.PropertyManagement.Core.Commands.DeleteFacility
{
    public class DeletePropertyCommand : IRequest
    {
        public int PropertyId { get; set; }
    }
}
