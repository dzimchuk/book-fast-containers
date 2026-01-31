using BookFast.Common.Application.Messaging;
using BookFast.PropertyManagement.Core.RentalProperties;

namespace BookFast.PropertyManagement.Core.RentalProperties.GetProperty
{
    public class GetPropertyQuery : IQuery<PropertyRepresentation>
    {
        public int Id { get; set; }
    }
}
