using BookFast.Common.Application.Messaging;

namespace BookFast.Common.Application.Queries
{
    public class QueryById<TKey, TRepresentation> : IQuery<TRepresentation>
    {
        public TKey Id { get; set; }
    }
}
