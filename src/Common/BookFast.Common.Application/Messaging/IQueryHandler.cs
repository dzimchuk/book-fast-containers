using BookFast.Common.SeedWork;
using MediatR;

namespace BookFast.Common.Application.Messaging;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
