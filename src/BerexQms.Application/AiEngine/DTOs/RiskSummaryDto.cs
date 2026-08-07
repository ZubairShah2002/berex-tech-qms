namespace BerexQms.Application.AiEngine.DTOs;

public sealed record RiskSummaryDto(
    int TotalRecommendations,
    int CriticalCount,
    int HighCount,
    int MediumCount,
    int LowCount,
    int PendingReview,
    int AcceptedCount,
    int RejectedCount,
    IReadOnlyList<RiskByModuleDto> RiskByModule,
    IReadOnlyList<RiskByTypeDto> RiskByType);

public sealed record RiskByModuleDto(
    string Module,
    int Count,
    int CriticalCount,
    int HighCount);

public sealed record RiskByTypeDto(
    string RecommendationType,
    int Count,
    decimal AverageConfidence);
