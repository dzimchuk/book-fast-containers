using MediatR;

namespace BookFast.SeedWork.Queries
{
    public class FindQuery<TKey, TRepresentation> : IRequest<TRepresentation>
    {
        public TKey Id { get; set; }
    }
}
