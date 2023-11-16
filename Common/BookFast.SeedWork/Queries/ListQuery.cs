using MediatR;

namespace BookFast.SeedWork.Queries
{
    public class ListQuery<TRepresentation> : IRequest<ListQueryResult<TRepresentation>>
    {
        public string OrderBy { get; set; }

        public OrderDirection? OrderDirection { get; set; }

        public int? PageNumber { get; set; }

        public int? PageSize { get; set; }
    }
}
