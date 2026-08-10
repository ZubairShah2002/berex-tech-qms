namespace BerexQms.Domain.AiEngine.Enums;

/// <summary>
/// Types of AI reasoning tasks. Used by the orchestrator to route
/// requests to the appropriate AI provider based on configuration.
/// </summary>
public enum AiTaskType
{
    DocumentAnalysis = 1,
    QualityAnalysis = 2,
    RecommendationGeneration = 3,
    Summarization = 4,
    RiskAnalysis = 5,
    CAPAAnalysis = 6,
    SupplierAnalysis = 7,
    AuditAnalysis = 8,
    DefectTrendAnalysis = 9,
    StructuredDataExtraction = 10,
}
