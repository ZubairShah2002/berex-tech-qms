using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Commands.UpdateCapabilityThresholds;

internal sealed class UpdateCapabilityThresholdsCommandHandler
    : ICommandHandler<UpdateCapabilityThresholdsCommand>
{
    private readonly IAiCapabilityConfigRepository _repository;

    public UpdateCapabilityThresholdsCommandHandler(IAiCapabilityConfigRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(
        UpdateCapabilityThresholdsCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AiCapabilityType>(request.Capability, true, out var capability))
            return Result.Failure(AiEngineErrors.InvalidCapability);

        var config = await _repository.GetByCapabilityAsync(capability.ToString(), cancellationToken);
        if (config is null)
            return Result.Failure(AiEngineErrors.CapabilityConfigNotFound);

        if (request.LowThreshold < AiCapabilityConfig.MinimumLowConfidenceThreshold ||
            request.LowThreshold >= request.ModerateThreshold ||
            request.ModerateThreshold >= request.HighThreshold ||
            request.HighThreshold > 1m)
        {
            return Result.Failure(AiEngineErrors.InvalidConfidenceThreshold);
        }

        config.UpdateThresholds(request.LowThreshold, request.ModerateThreshold, request.HighThreshold);

        await _repository.UpdateAsync(config, cancellationToken);

        return Result.Success();
    }
}
