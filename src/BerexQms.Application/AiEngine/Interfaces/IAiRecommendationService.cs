using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Interfaces;

/// <summary>
/// AI Recommendation Service — generates quality intelligence by analysing
/// QMS context documents and knowledge sources. All data retrieval flows
/// through the AI Context Engine; no direct business module access.
/// </summary>
public interface IAiRecommendationService
{
    /// <summary>
    /// Generates recommendations by analysing context documents for a given module.
    /// Returns structured, explainable recommendations with supporting evidence.
    /// </summary>
    Task<IReadOnlyList<QualityInsightDto>> GenerateRecommendationsAsync(
        string? module, string? recommendationType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyses quality trends from context documents — defect frequency,
    /// trend direction, product/process correlations.
    /// </summary>
    Task<IReadOnlyList<QualityInsightDto>> AnalyseQualityTrendAsync(
        string? module, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a risk assessment across modules by evaluating context documents
    /// for severity indicators, trend direction, and cross-module correlations.
    /// </summary>
    Task<IReadOnlyList<QualityInsightDto>> GenerateRiskAssessmentAsync(
        string? module, CancellationToken cancellationToken = default);
}
