using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.AiEngine.Commands.CreateRecommendation;

public sealed record CreateRecommendationCommand(
    string RecommendationType,
    string Title,
    string Description,
    string Severity,
    string RelatedModule,
    string? RelatedEntityId,
    decimal ConfidenceScore,
    string Reason,
    string? SupportingData,
    string? RecommendedAction,
    string? SourceContextIds) : ICommand<Guid>;
