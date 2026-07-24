using BerexQms.SharedKernel.Results;
using MediatR;

namespace BerexQms.Application.Abstractions.Messaging;

/// <summary>
/// Query interface that returns a <see cref="Result{TResponse}"/> envelope.
/// Queries are read-only operations and must not mutate state.
/// </summary>
/// <typeparam name="TResponse">The type of the response payload.</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
