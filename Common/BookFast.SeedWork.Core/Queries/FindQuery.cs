using MediatR;

namespace BookFast.SeedWork.Core.Queries
{
    public class FindQuery<TKey, TRepresentation> : IRequest<TRepresentation>
    {
        public TKey Id { get; set; }
    }
}
