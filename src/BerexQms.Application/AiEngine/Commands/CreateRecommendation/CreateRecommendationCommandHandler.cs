using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Commands.CreateRecommendation;

internal sealed class CreateRecommendationCommandHandler
    : ICommandHandler<CreateRecommendationCommand, Guid>
{
    private readonly IAiRecommendationRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CreateRecommendationCommandHandler(
        IAiRecommendationRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(
        CreateRecommendationCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AiRecommendationType>(request.RecommendationType, true, out var recType))
            return AiEngineErrors.InvalidRecommendationType;

        if (!Enum.TryParse<AiSeverity>(request.Severity, true, out var severity))
            return AiEngineErrors.InvalidSeverity;

        var recommendation = AiRecommendation.Create(
            Guid.NewGuid(),
            _tenantContext.CurrentTenantId,
            recType,
            request.Title,
            request.Description,
            severity,
            request.RelatedModule,
            request.RelatedEntityId,
            request.ConfidenceScore,
            request.Reason,
            request.SupportingData,
            request.RecommendedAction,
            request.SourceContextIds);

        await _repository.AddAsync(recommendation, cancellationToken);

        return recommendation.Id;
    }
}
