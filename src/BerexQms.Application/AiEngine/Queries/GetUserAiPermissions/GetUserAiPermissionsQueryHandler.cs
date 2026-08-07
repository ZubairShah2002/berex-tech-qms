using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.Domain.AiEngine;
using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Queries.GetUserAiPermissions;

internal sealed class GetUserAiPermissionsQueryHandler
    : IQueryHandler<GetUserAiPermissionsQuery, AiUserPermissionsDto>
{
    private readonly IAiPermissionService _permissionService;
    private readonly IAiPermissionPolicyRepository _policyRepository;

    public GetUserAiPermissionsQueryHandler(
        IAiPermissionService permissionService,
        IAiPermissionPolicyRepository policyRepository)
    {
        _permissionService = permissionService;
        _policyRepository = policyRepository;
    }

    public async Task<Result<AiUserPermissionsDto>> Handle(
        GetUserAiPermissionsQuery request, CancellationToken cancellationToken)
    {
        var level = await _permissionService.GetUserPermissionLevelAsync(
            request.UserId, cancellationToken);

        var policy = await _policyRepository.GetActiveByUserAsync(
            request.UserId, cancellationToken);

        var allowedActions = Enum.GetValues<AiActionType>()
            .Where(a => AiActionPolicy.IsAuthorized(level, a))
            .Select(a => a.ToString())
            .ToArray();

        var allowedCategories = Enum.GetValues<AiActionType>()
            .Where(a => AiActionPolicy.IsAuthorized(level, a))
            .Select(a => AiActionPolicy.GetCategory(a).ToString())
            .Distinct()
            .ToArray();

        return new AiUserPermissionsDto(
            request.UserId,
            level.ToString(),
            (int)level,
            allowedActions,
            allowedCategories,
            policy is not null);
    }
}
