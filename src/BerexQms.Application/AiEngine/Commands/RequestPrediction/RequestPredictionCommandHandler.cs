using System.Diagnostics;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Commands.RequestPrediction;

/// <summary>
/// Requests an AI-generated suggestion for the given capability. The external ML model
/// service is not yet connected in this environment, so the handler records a real
/// <see cref="AiInteraction"/> against the tenant's active model but completes it with a
/// placeholder, zero-confidence (and therefore suppressed) response until model serving
/// is wired up in Infrastructure.
/// </summary>
internal sealed class RequestPredictionCommandHandler
    : ICommandHandler<RequestPredictionCommand, AiSuggestionDto>
{
    private readonly IAiInteractionRepository _interactionRepository;
    private readonly IAiModelRepository _modelRepository;
    private readonly IAiCapabilityConfigRepository _capabilityConfigRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public RequestPredictionCommandHandler(
        IAiInteractionRepository interactionRepository,
        IAiModelRepository modelRepository,
        IAiCapabilityConfigRepository capabilityConfigRepository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _interactionRepository = interactionRepository;
        _modelRepository = modelRepository;
        _capabilityConfigRepository = capabilityConfigRepository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AiSuggestionDto>> Handle(
        RequestPredictionCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AiCapabilityType>(request.Capability, true, out var capability))
            return AiEngineErrors.InvalidCapability;

        var capabilityName = capability.ToString();

        var config = await _capabilityConfigRepository.GetByCapabilityAsync(capabilityName, cancellationToken);
        if (config is null)
            return AiEngineErrors.CapabilityConfigNotFound;

        if (!config.IsEnabled)
            return AiEngineErrors.CapabilityDisabled;

        var activeModel = await _modelRepository.GetActiveModelAsync(capabilityName, cancellationToken);
        if (activeModel is null)
            return AiEngineErrors.ModelNotFound;

        var stopwatch = Stopwatch.StartNew();

        var inputSummary = request.RelatedRecordId.HasValue || request.RelatedRecordType is not null
            ? $"{request.InputContext} [Related: {request.RelatedRecordType ?? "Unknown"}#{request.RelatedRecordId}]".Trim()
            : request.InputContext;

        var interaction = AiInteraction.Create(
            Guid.NewGuid(),
            _tenantContext.CurrentTenantId,
            capability,
            _currentUserService.UserId,
            activeModel.Id.ToString(),
            inputSummary);

        // The external model-serving integration is not connected yet. Complete the
        // interaction with a zero-confidence placeholder response — automatically
        // suppressed per ConfidenceScore's own threshold — rather than a real prediction,
        // so the capability's configuration and audit trail can still be exercised
        // end-to-end.
        var placeholderOutput =
            $"The '{capabilityName}' AI capability is configured with model " +
            $"'{activeModel.Name} v{activeModel.Version}', but the external model " +
            "service is not currently connected. This is a placeholder response.";

        stopwatch.Stop();

        interaction.Complete(
            placeholderOutput,
            confidenceScore: 0m,
            sourceReferences: null,
            responseTimeMs: (int)stopwatch.ElapsedMilliseconds);

        await _interactionRepository.AddAsync(interaction, cancellationToken);

        var suggestion = new AiSuggestionDto(
            interaction.Id,
            capabilityName,
            interaction.OutputSummary,
            interaction.Confidence?.Score,
            interaction.Confidence?.Level.ToString(),
            [],
            IsSuppressed: interaction.Confidence?.IsSuppressed ?? true);

        return suggestion;
    }
}
