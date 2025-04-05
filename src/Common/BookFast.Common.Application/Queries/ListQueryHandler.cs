using BookFast.Common.Application.Messaging;
using BookFast.Common.SeedWork;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookFast.Common.Application.Queries
{
    public abstract class ListQueryHandler<TQuery, TRepresentation> : IQueryHandler<TQuery, ListQueryResult<TRepresentation>>
        where TQuery : ListQuery<TRepresentation>
    {
        protected abstract IQueryable<TRepresentation> FilterAndProject(TQuery request);
        protected abstract Dictionary<string, Expression<Func<TRepresentation, object>>> GetOrderingExpressionMap();
        protected abstract string GetDefaultOrderField();
        protected virtual string GetAdditionalOrderField() => string.Empty;

        public async Task<Result<ListQueryResult<TRepresentation>>> Handle(TQuery request, CancellationToken cancellationToken)
        {
            var query = FilterAndProject(request);

            var totalRecordsCount = await query.CountAsync(cancellationToken);

            query = OrderBy(query, request);

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

        public virtual IOrderedQueryable<TRepresentation> OrderBy(IQueryable<TRepresentation> query, TQuery request)
        {
            var orderingExpressionMap = GetOrderingExpressionMap().ToDictionary(pair => pair.Key.ToLowerInvariant(), pair => pair.Value);

            var orderByField = !string.IsNullOrWhiteSpace(request.OrderBy) && orderingExpressionMap.ContainsKey(request.OrderBy.ToLowerInvariant())
                ? request.OrderBy.ToLowerInvariant()
                : GetDefaultOrderField().ToLowerInvariant();
            var orderBy = orderingExpressionMap[orderByField];

            var orderDirection = request.OrderDirection ?? OrderDirection.Asc;

            var orderedQuery = orderDirection == OrderDirection.Asc
                    ? query.OrderBy(orderBy)
                    : query.OrderByDescending(orderBy);

            string additionalOrderField = GetAdditionalOrderField();
            if (!string.IsNullOrWhiteSpace(additionalOrderField))
            {
                var thenBy = orderingExpressionMap[additionalOrderField.ToLowerInvariant()];
                orderedQuery = orderDirection == OrderDirection.Asc
                    ? orderedQuery.ThenBy(thenBy)
                    : orderedQuery.ThenByDescending(thenBy);
            }
            else
            {
                var isDefaultOrdering = orderByField.Equals(GetDefaultOrderField(), StringComparison.InvariantCultureIgnoreCase);

                if (!isDefaultOrdering)
                {
                    var defaultOrderingField = orderingExpressionMap[GetDefaultOrderField().ToLowerInvariant()];
                    orderedQuery = orderDirection == OrderDirection.Asc
                        ? orderedQuery.ThenBy(defaultOrderingField)
                        : orderedQuery.ThenByDescending(defaultOrderingField);
                }
            }

            return orderedQuery;
        }

        private static int CalculateNumberOfPages(int pageSize, int totalRecords)
        {
            return (int)Math.Ceiling((double)totalRecords / pageSize);
        }
    }
}
