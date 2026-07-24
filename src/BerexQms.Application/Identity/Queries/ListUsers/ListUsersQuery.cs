using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Identity.DTOs;

namespace BerexQms.Application.Identity.Queries.ListUsers;

public sealed record ListUsersQuery(
    string? SearchTerm = null,
    string? Status = null,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<UserDto>>;
