using BookFast.Common.SeedWork;
using MediatR;

namespace BookFast.Common.Application.Messaging;

public interface ICommand : IRequest<Result>, IValidatableRequest;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IValidatableRequest;

public interface IValidatableRequest;
