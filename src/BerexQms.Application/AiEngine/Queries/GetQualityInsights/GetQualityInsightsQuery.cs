using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Queries.GetQualityInsights;

public sealed record GetQualityInsightsQuery(
    string? Module,
    string? AnalysisType) : IQuery<IReadOnlyList<QualityInsightDto>>;
