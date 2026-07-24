using BerexQms.SharedKernel.Results;
using MediatR;

namespace BerexQms.Application.Abstractions.Messaging;

/// <summary>
/// Handler for commands that return a non-generic <see cref="Result"/>.
/// </summary>
/// <typeparam name="TCommand">The command type being handled.</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

/// <summary>
/// Handler for commands that return a <see cref="Result{TResponse}"/> envelope.
/// </summary>
/// <typeparam name="TCommand">The command type being handled.</typeparam>
/// <typeparam name="TResponse">The type of the response payload.</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;
