using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Identity.DTOs;

namespace BerexQms.Application.Identity.Commands.LoginUser;

public sealed record LoginUserCommand(
    string Email,
    string Password) : ICommand<AuthTokenDto>;
