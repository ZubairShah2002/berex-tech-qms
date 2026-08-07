using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Commands.ToggleCapability;

internal sealed class ToggleCapabilityCommandHandler : ICommandHandler<ToggleCapabilityCommand>
{
    private readonly IAiCapabilityConfigRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public ToggleCapabilityCommandHandler(
        IAiCapabilityConfigRepository repository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(ToggleCapabilityCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AiCapabilityType>(request.Capability, true, out var capability))
            return Result.Failure(AiEngineErrors.InvalidCapability);

        var config = await _repository.GetByCapabilityAsync(capability.ToString(), cancellationToken);
        var isNew = config is null;

        config ??= AiCapabilityConfig.Create(Guid.NewGuid(), _tenantContext.CurrentTenantId, capability);

        if (request.Enable)
            config.Enable(_currentUserService.UserId);
        else
            config.Disable(_currentUserService.UserId);

        if (isNew)
            await _repository.AddAsync(config, cancellationToken);
        else
            await _repository.UpdateAsync(config, cancellationToken);

        return Result.Success();
    }
}
