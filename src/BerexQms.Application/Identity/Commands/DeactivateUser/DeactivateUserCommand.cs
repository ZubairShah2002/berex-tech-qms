using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Identity.Commands.DeactivateUser;

public sealed record DeactivateUserCommand(Guid UserId) : ICommand;
