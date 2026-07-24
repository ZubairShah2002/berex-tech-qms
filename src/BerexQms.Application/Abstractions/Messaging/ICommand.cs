using BerexQms.SharedKernel.Results;
using MediatR;

namespace BerexQms.Application.Abstractions.Messaging;

/// <summary>
/// Marker interface for commands that return a non-generic <see cref="Result"/>.
/// </summary>
public interface ICommand : IRequest<Result>;

/// <summary>
/// Generic command interface that returns a <see cref="Result{TResponse}"/> envelope.
/// </summary>
/// <typeparam name="TResponse">The type of the response payload.</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
