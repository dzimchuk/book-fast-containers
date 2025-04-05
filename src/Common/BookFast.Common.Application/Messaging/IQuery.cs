using BookFast.Common.SeedWork;
using MediatR;

namespace BookFast.Common.Application.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
