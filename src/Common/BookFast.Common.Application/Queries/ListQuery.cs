using BookFast.Common.Application.Messaging;

namespace BookFast.Common.Application.Queries
{
    public class ListQuery<TRepresentation> : IQuery<ListQueryResult<TRepresentation>>
    {
        public string OrderBy { get; set; }

        public OrderDirection? OrderDirection { get; set; }

        public int? PageNumber { get; set; }

        public int? PageSize { get; set; }
    }
}
