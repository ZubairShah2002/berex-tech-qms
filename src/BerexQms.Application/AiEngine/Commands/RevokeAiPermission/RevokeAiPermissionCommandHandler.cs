using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Commands.RevokeAiPermission;

internal sealed class RevokeAiPermissionCommandHandler
    : ICommandHandler<RevokeAiPermissionCommand>
{
    private readonly IAiPermissionPolicyRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public RevokeAiPermissionCommandHandler(
        IAiPermissionPolicyRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(
        RevokeAiPermissionCommand request, CancellationToken cancellationToken)
    {
        var policy = await _repository.GetActiveByUserAsync(request.UserId, cancellationToken);

        if (policy is null)
            return Result.Failure(AiEngineErrors.PermissionPolicyNotFound);

        policy.Revoke(_currentUserService.UserId);
        await _repository.UpdateAsync(policy, cancellationToken);

        return Result.Success();
    }
}
