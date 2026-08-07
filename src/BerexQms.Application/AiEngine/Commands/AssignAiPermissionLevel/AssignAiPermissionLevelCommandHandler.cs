using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Commands.AssignAiPermissionLevel;

internal sealed class AssignAiPermissionLevelCommandHandler
    : ICommandHandler<AssignAiPermissionLevelCommand>
{
    private readonly IAiPermissionPolicyRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public AssignAiPermissionLevelCommandHandler(
        IAiPermissionPolicyRepository repository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(
        AssignAiPermissionLevelCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AiPermissionLevel>(request.PermissionLevel, true, out var level))
            return Result.Failure(AiEngineErrors.InvalidPermissionLevel);

        var existing = await _repository.GetActiveByUserAsync(request.UserId, cancellationToken);

        if (existing is not null)
        {
            existing.UpdateLevel(level, _currentUserService.UserId);
            await _repository.UpdateAsync(existing, cancellationToken);
        }
        else
        {
            var policy = AiPermissionPolicy.Create(
                Guid.NewGuid(),
                _tenantContext.CurrentTenantId,
                request.UserId,
                level,
                _currentUserService.UserId,
                request.Notes);

            await _repository.AddAsync(policy, cancellationToken);
        }

        return Result.Success();
    }
}
