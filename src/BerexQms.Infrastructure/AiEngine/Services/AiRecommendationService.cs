using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Repositories;
using Microsoft.Extensions.Logging;

namespace BerexQms.Infrastructure.AiEngine.Services;

/// <summary>
/// AI Recommendation Service — analyses context documents from the AI Context
/// Engine to generate quality intelligence. All analysis is performed on the
/// context layer; no direct business module access.
///
/// This implementation performs rule-based analysis on context documents.
/// A future phase will integrate with an ML model for deeper pattern detection.
/// </summary>
internal sealed class AiRecommendationService : IAiRecommendationService
{
    private readonly IAiContextDocumentRepository _contextDocumentRepository;
    private readonly IAiKnowledgeSourceRepository _knowledgeSourceRepository;
    private readonly ILogger<AiRecommendationService> _logger;

    public AiRecommendationService(
        IAiContextDocumentRepository contextDocumentRepository,
        IAiKnowledgeSourceRepository knowledgeSourceRepository,
        ILogger<AiRecommendationService> logger)
    {
        _contextDocumentRepository = contextDocumentRepository;
        _knowledgeSourceRepository = knowledgeSourceRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<QualityInsightDto>> GenerateRecommendationsAsync(
        string? module, string? recommendationType, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Generating recommendations for module={Module}, type={Type}",
            module ?? "all", recommendationType ?? "all");

        var documents = await GetFilteredDocumentsAsync(module, cancellationToken);
        var insights = new List<QualityInsightDto>();

        // Analyse documents by context type to generate category-specific recommendations
        var grouped = documents.GroupBy(d => d.ContextType);

        foreach (var group in grouped)
        {
            var contextType = group.Key;
            var docs = group.ToList();

            if (ShouldAnalyse(recommendationType, "DefectTrend") &&
                contextType == AiContextType.Quality.ToString())
            {
                insights.AddRange(AnalyseDefectTrends(docs));
            }

            if (ShouldAnalyse(recommendationType, "SupplierRisk") &&
                contextType == AiContextType.Supplier.ToString())
            {
                insights.AddRange(AnalyseSupplierRisks(docs));
            }

            if (ShouldAnalyse(recommendationType, "ProcessRisk") &&
                (contextType == AiContextType.Quality.ToString() ||
                 contextType == AiContextType.NonConformance.ToString()))
            {
                insights.AddRange(AnalyseProcessRisks(docs));
            }

            if (ShouldAnalyse(recommendationType, "DocumentGap") &&
                contextType == AiContextType.Document.ToString())
            {
                insights.AddRange(AnalyseDocumentGaps(docs));
            }

            if (ShouldAnalyse(recommendationType, "AuditRisk") &&
                contextType == AiContextType.Audit.ToString())
            {
                insights.AddRange(AnalyseAuditRisks(docs));
            }

            if (ShouldAnalyse(recommendationType, "CAPARecommendation") &&
                contextType == AiContextType.CorrectiveAction.ToString())
            {
                insights.AddRange(AnalyseCAPAEffectiveness(docs));
            }
        }

        _logger.LogDebug("Generated {Count} recommendations from {DocCount} context documents",
            insights.Count, documents.Count);

        return insights;
    }

    public async Task<IReadOnlyList<QualityInsightDto>> AnalyseQualityTrendAsync(
        string? module, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Analysing quality trends for module={Module}", module ?? "all");

        var documents = await GetFilteredDocumentsAsync(module, cancellationToken);
        var insights = new List<QualityInsightDto>();

        // Quality trend analysis focuses on Quality, NonConformance, and CorrectiveAction context
        var qualityDocs = documents.Where(d =>
            d.ContextType == AiContextType.Quality.ToString() ||
            d.ContextType == AiContextType.NonConformance.ToString() ||
            d.ContextType == AiContextType.CorrectiveAction.ToString())
            .ToList();

        if (qualityDocs.Count == 0)
        {
            insights.Add(new QualityInsightDto(
                "NoData",
                "Insufficient Quality Data",
                "No quality-related context documents are available for trend analysis. " +
                "Index quality inspection, NCR, and CAPA data to enable trend detection.",
                AiSeverity.Low.ToString(),
                0.0m,
                module ?? "All",
                null,
                DateTime.UtcNow));

            return insights;
        }

        insights.AddRange(AnalyseDefectTrends(
            qualityDocs.Where(d => d.ContextType == AiContextType.Quality.ToString()).ToList()));
        insights.AddRange(AnalyseProcessRisks(
            qualityDocs.Where(d => d.ContextType == AiContextType.NonConformance.ToString()).ToList()));
        insights.AddRange(AnalyseCAPAEffectiveness(
            qualityDocs.Where(d => d.ContextType == AiContextType.CorrectiveAction.ToString()).ToList()));

        return insights;
    }

    public async Task<IReadOnlyList<QualityInsightDto>> GenerateRiskAssessmentAsync(
        string? module, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Generating risk assessment for module={Module}", module ?? "all");

        var documents = await GetFilteredDocumentsAsync(module, cancellationToken);
        var insights = new List<QualityInsightDto>();

        // Cross-module risk assessment
        insights.AddRange(AnalyseSupplierRisks(
            documents.Where(d => d.ContextType == AiContextType.Supplier.ToString()).ToList()));
        insights.AddRange(AnalyseAuditRisks(
            documents.Where(d => d.ContextType == AiContextType.Audit.ToString()).ToList()));
        insights.AddRange(AnalyseDocumentGaps(
            documents.Where(d => d.ContextType == AiContextType.Document.ToString()).ToList()));

        // Assess overall knowledge coverage
        var activeSources = await _knowledgeSourceRepository.GetActiveSourcesAsync(cancellationToken);
        if (activeSources.Count < 3)
        {
            insights.Add(new QualityInsightDto(
                "KnowledgeCoverage",
                "Limited Knowledge Source Coverage",
                $"Only {activeSources.Count} knowledge source(s) are active. " +
                "Broader coverage across QMS modules improves risk detection accuracy.",
                AiSeverity.Medium.ToString(),
                0.60m,
                module ?? "All",
                $"Active sources: {string.Join(", ", activeSources.Select(s => s.Module))}",
                DateTime.UtcNow));
        }

        return insights;
    }

    // ---- Private analysis methods ----

    private async Task<IReadOnlyList<AiContextDocument>> GetFilteredDocumentsAsync(
        string? module, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(module))
        {
            return await _contextDocumentRepository.GetByModuleAsync(module, cancellationToken);
        }

        return await _contextDocumentRepository.ListAllAsync(cancellationToken);
    }

    private static bool ShouldAnalyse(string? requestedType, string targetType)
    {
        return string.IsNullOrWhiteSpace(requestedType) ||
               requestedType.Equals(targetType, StringComparison.OrdinalIgnoreCase);
    }

    private static List<QualityInsightDto> AnalyseDefectTrends(IReadOnlyList<AiContextDocument> docs)
    {
        var insights = new List<QualityInsightDto>();
        if (docs.Count == 0) return insights;

        // Group by source module to detect per-module defect concentration
        var byModule = docs.GroupBy(d => d.SourceModule);
        foreach (var group in byModule)
        {
            var count = group.Count();
            if (count >= 3)
            {
                insights.Add(new QualityInsightDto(
                    "DefectTrend",
                    $"Defect Data Concentration — {group.Key}",
                    $"{count} quality context documents detected for {group.Key}. " +
                    "Review defect frequency and distribution for emerging trends.",
                    count >= 10 ? AiSeverity.High.ToString() : AiSeverity.Medium.ToString(),
                    Math.Min(0.95m, 0.50m + count * 0.05m),
                    group.Key,
                    $"Document count: {count}. Earliest: {group.Min(d => d.CreatedAt):yyyy-MM-dd}. " +
                    $"Latest: {group.Max(d => d.CreatedAt):yyyy-MM-dd}.",
                    DateTime.UtcNow));
            }
        }

        return insights;
    }

    private static List<QualityInsightDto> AnalyseSupplierRisks(IReadOnlyList<AiContextDocument> docs)
    {
        var insights = new List<QualityInsightDto>();
        if (docs.Count == 0) return insights;

        // Multiple supplier-context documents may indicate elevated supplier risk
        if (docs.Count >= 2)
        {
            insights.Add(new QualityInsightDto(
                "SupplierRisk",
                "Supplier Quality Risk Indicator",
                $"{docs.Count} supplier quality context documents indexed. " +
                "Review supplier reject rates, incoming inspection failures, and NCR frequency " +
                "to identify suppliers requiring corrective action.",
                docs.Count >= 5 ? AiSeverity.High.ToString() : AiSeverity.Medium.ToString(),
                Math.Min(0.90m, 0.50m + docs.Count * 0.08m),
                "SupplierQuality",
                $"Supplier contexts: {docs.Count}. " +
                $"Modules covered: {string.Join(", ", docs.Select(d => d.SourceModule).Distinct())}.",
                DateTime.UtcNow));
        }

        return insights;
    }

    private static List<QualityInsightDto> AnalyseProcessRisks(IReadOnlyList<AiContextDocument> docs)
    {
        var insights = new List<QualityInsightDto>();
        if (docs.Count == 0) return insights;

        if (docs.Count >= 2)
        {
            insights.Add(new QualityInsightDto(
                "ProcessRisk",
                "Process Non-Conformance Pattern Detected",
                $"{docs.Count} non-conformance related context documents found. " +
                "Analyse root cause patterns, repeat defect types, and affected processes " +
                "to identify systemic process risks.",
                docs.Count >= 8 ? AiSeverity.High.ToString() : AiSeverity.Medium.ToString(),
                Math.Min(0.90m, 0.45m + docs.Count * 0.06m),
                docs.FirstOrDefault()?.SourceModule ?? "Unknown",
                $"NCR contexts: {docs.Count}.",
                DateTime.UtcNow));
        }

        return insights;
    }

    private static List<QualityInsightDto> AnalyseDocumentGaps(IReadOnlyList<AiContextDocument> docs)
    {
        var insights = new List<QualityInsightDto>();
        if (docs.Count == 0) return insights;

        // Check for stale documents that may indicate outdated procedures
        var staleCount = docs.Count(d =>
            d.EmbeddingStatus == AiEmbeddingStatus.Stale.ToString());

        if (staleCount > 0)
        {
            insights.Add(new QualityInsightDto(
                "DocumentGap",
                "Stale Document Content Detected",
                $"{staleCount} document context(s) have been modified since last indexing. " +
                "Re-index to ensure AI recommendations reflect current procedures and standards.",
                staleCount >= 5 ? AiSeverity.High.ToString() : AiSeverity.Low.ToString(),
                0.75m,
                "DocumentControl",
                $"Stale documents: {staleCount} of {docs.Count} total.",
                DateTime.UtcNow));
        }

        return insights;
    }

    private static List<QualityInsightDto> AnalyseAuditRisks(IReadOnlyList<AiContextDocument> docs)
    {
        var insights = new List<QualityInsightDto>();
        if (docs.Count == 0) return insights;

        if (docs.Count >= 2)
        {
            insights.Add(new QualityInsightDto(
                "AuditRisk",
                "Audit Attention Required",
                $"{docs.Count} audit-related context documents indexed. " +
                "Review for open findings, repeated observations, and areas requiring " +
                "additional audit attention.",
                docs.Count >= 5 ? AiSeverity.High.ToString() : AiSeverity.Medium.ToString(),
                Math.Min(0.85m, 0.50m + docs.Count * 0.07m),
                "AuditManagement",
                $"Audit contexts: {docs.Count}.",
                DateTime.UtcNow));
        }

        return insights;
    }

    private static List<QualityInsightDto> AnalyseCAPAEffectiveness(IReadOnlyList<AiContextDocument> docs)
    {
        var insights = new List<QualityInsightDto>();
        if (docs.Count == 0) return insights;

        if (docs.Count >= 2)
        {
            insights.Add(new QualityInsightDto(
                "CAPARecommendation",
                "CAPA Effectiveness Review Recommended",
                $"{docs.Count} corrective action context documents found. " +
                "Evaluate whether previous corrective actions have reduced repeat defects " +
                "and whether closure effectiveness meets quality objectives.",
                docs.Count >= 5 ? AiSeverity.High.ToString() : AiSeverity.Medium.ToString(),
                Math.Min(0.85m, 0.50m + docs.Count * 0.07m),
                "Capa",
                $"CAPA contexts: {docs.Count}.",
                DateTime.UtcNow));
        }

        return insights;
    }
}
