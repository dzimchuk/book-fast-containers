using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookFast.SeedWork.Core.Queries
{
    public abstract class ListQueryHandler<TQuery, TRepresentation> : IRequestHandler<TQuery, ListQueryResult<TRepresentation>>
        where TQuery : ListQuery<TRepresentation>
    {
        protected abstract IQueryable<TRepresentation> FilterAndProject(TQuery request);
        protected abstract Dictionary<string, Expression<Func<TRepresentation, object>>> GetOrderingExpressionMap();
        protected abstract string GetDefaultOrderField();

        public async Task<ListQueryResult<TRepresentation>> Handle(TQuery request, CancellationToken cancellationToken)
        {
            var query = FilterAndProject(request);

            var totalRecordsCount = await query.CountAsync(cancellationToken);

            var orderingExpressionMap = GetOrderingExpressionMap().ToDictionary(pair => pair.Key.ToLowerInvariant(), pair => pair.Value);

            var orderBy = !string.IsNullOrWhiteSpace(request.OrderBy) && orderingExpressionMap.ContainsKey(request.OrderBy.ToLowerInvariant())
                ? orderingExpressionMap[request.OrderBy.ToLowerInvariant()]
                : orderingExpressionMap[GetDefaultOrderField().ToLowerInvariant()];
            var orderDirection = request.OrderDirection ?? OrderDirection.Asc;

            query = orderDirection == OrderDirection.Asc
                    ? query.OrderBy(orderBy)
                    : query.OrderByDescending(orderBy);

            if (request?.PageNumber is not null && request?.PageSize is not null)
            {
                var toSkip = (request.PageNumber.Value - 1) * request.PageSize.Value;

                query = query.Skip(toSkip).Take(request.PageSize.Value);
            }

            return new ListQueryResult<TRepresentation>
            {
                Records = await query.ToListAsync(cancellationToken),
                PageNumber = request.PageNumber,
                TotalRecords = totalRecordsCount,
                TotalPages = request.PageSize.HasValue
                    ? CalculateNumberOfPages(request.PageSize.Value, totalRecordsCount)
                    : null
            };
        }

        private static int CalculateNumberOfPages(int pageSize, int totalRecords)
        {
            return (int)Math.Ceiling((double)totalRecords / pageSize);
        }
    }
}
