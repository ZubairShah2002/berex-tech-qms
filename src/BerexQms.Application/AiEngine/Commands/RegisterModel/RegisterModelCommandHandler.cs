using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Commands.RegisterModel;

internal sealed class RegisterModelCommandHandler : ICommandHandler<RegisterModelCommand, Guid>
{
    private readonly IAiModelRepository _repository;
    private readonly ITenantContext _tenantContext;

    public RegisterModelCommandHandler(IAiModelRepository repository, ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(RegisterModelCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AiCapabilityType>(request.Capability, true, out var capability))
            return AiEngineErrors.InvalidCapability;

        if (await _repository.VersionExistsAsync(request.Name, request.Version, cancellationToken))
            return AiEngineErrors.ModelVersionExists;

        var model = AiModel.Create(
            Guid.NewGuid(),
            _tenantContext.CurrentTenantId,
            request.Name,
            request.Version,
            capability,
            request.Description,
            trainingMetrics: null,
            request.HyperParameters,
            dataSnapshotReference: null,
            trainingSampleCount: null);

        await _repository.AddAsync(model, cancellationToken);

        return model.Id;
    }
}
