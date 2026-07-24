using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Identity.DTOs;

namespace BerexQms.Application.Identity.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? Department,
    string? JobTitle) : ICommand<UserDto>;
