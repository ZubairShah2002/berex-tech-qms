using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Identity.DTOs;

namespace BerexQms.Application.Identity.Commands.RegisterUser;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber = null,
    string? Department = null,
    string? JobTitle = null,
    List<Guid>? RoleIds = null) : ICommand<UserDto>;
