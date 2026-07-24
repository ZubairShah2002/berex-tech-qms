using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Identity.Commands.ChangePassword;

public sealed record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword) : ICommand;
