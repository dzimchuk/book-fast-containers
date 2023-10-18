using BookFast.PropertyManagement.Core.Queries.Representations;

namespace BookFast.PropertyManagement.Core.Queries.GetProperty
{
    public class GetPropertyQuery : IRequest<PropertyRepresentation>
    {
        public int Id { get; set; }
    }
}
