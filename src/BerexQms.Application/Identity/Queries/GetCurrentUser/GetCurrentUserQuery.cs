using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Identity.DTOs;

namespace BerexQms.Application.Identity.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IQuery<UserDto>;
