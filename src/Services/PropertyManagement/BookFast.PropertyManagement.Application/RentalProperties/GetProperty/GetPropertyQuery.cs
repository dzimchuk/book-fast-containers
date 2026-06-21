using BookFast.Common.Application.Messaging;

namespace BookFast.PropertyManagement.Application.RentalProperties.GetProperty
{
    public class GetPropertyQuery : IQuery<PropertyRepresentation>
    {
        public Guid Id { get; set; }
    }
}
