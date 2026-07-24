using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Identity.Commands.ActivateUser;

public sealed record ActivateUserCommand(Guid UserId) : ICommand;
