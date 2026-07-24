using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Identity.DTOs;
using BerexQms.Domain.Identity.Entities;
using BerexQms.Domain.Identity.Enums;
using BerexQms.Domain.Identity.Repositories;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Identity.Queries.ListUsers;

public sealed class ListUsersQueryHandler : IQueryHandler<ListUsersQuery, PagedResult<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public ListUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<PagedResult<UserDto>>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        var spec = new UserListSpecification(request.SearchTerm, request.Status, request.Page, request.PageSize);
        var users = await _userRepository.ListAsync(spec, cancellationToken);

        var countSpec = new UserCountSpecification(request.SearchTerm, request.Status);
        var totalCount = await _userRepository.CountAsync(countSpec, cancellationToken);

        var dtos = users.Select(u => new UserDto(
            u.Id,
            u.Email.Value,
            u.Name.FirstName,
            u.Name.LastName,
            u.Name.DisplayName,
            u.Status.ToString(),
            u.PhoneNumber,
            u.Department,
            u.JobTitle,
            u.LastLoginAt,
            u.UserRoles.Select(ur => ur.Role.Name).ToList(),
            u.CreatedAt)).ToList();

        return new PagedResult<UserDto>(dtos, totalCount, request.Page, request.PageSize);
    }

    private sealed class UserListSpecification : Specification<User>
    {
        public UserListSpecification(string? searchTerm, string? status, int page, int pageSize)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<UserStatus>(status, true, out var parsed))
                {
                    ApplyCriteria(u =>
                        (u.Email.Value.Contains(term) ||
                         u.Name.FirstName.ToLower().Contains(term) ||
                         u.Name.LastName.ToLower().Contains(term)) &&
                        u.Status == parsed);
                }
                else
                {
                    ApplyCriteria(u =>
                        u.Email.Value.Contains(term) ||
                        u.Name.FirstName.ToLower().Contains(term) ||
                        u.Name.LastName.ToLower().Contains(term));
                }
            }
            else if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<UserStatus>(status, true, out var parsed))
            {
                ApplyCriteria(u => u.Status == parsed);
            }

            ApplyOrderBy(u => u.Name.LastName);
            ApplyPaging((page - 1) * pageSize, pageSize);
        }
    }

    private sealed class UserCountSpecification : Specification<User>
    {
        public UserCountSpecification(string? searchTerm, string? status)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<UserStatus>(status, true, out var parsed))
                {
                    ApplyCriteria(u =>
                        (u.Email.Value.Contains(term) ||
                         u.Name.FirstName.ToLower().Contains(term) ||
                         u.Name.LastName.ToLower().Contains(term)) &&
                        u.Status == parsed);
                }
                else
                {
                    ApplyCriteria(u =>
                        u.Email.Value.Contains(term) ||
                        u.Name.FirstName.ToLower().Contains(term) ||
                        u.Name.LastName.ToLower().Contains(term));
                }
            }
            else if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<UserStatus>(status, true, out var parsed))
            {
                ApplyCriteria(u => u.Status == parsed);
            }
        }
    }
}
