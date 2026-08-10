namespace BerexQms.Domain.AiEngine.Enums;

/// <summary>
/// Classifies the type of AI-generated quality recommendation.
/// Each type targets a specific domain of QMS intelligence.
/// </summary>
public enum AiRecommendationType
{
    /// <summary>Trends detected in defect frequency, percentage, or distribution.</summary>
    DefectTrend = 1,

    /// <summary>Risk identified in supplier quality, reject rates, or SCAR history.</summary>
    SupplierRisk = 2,

    /// <summary>Risk identified in manufacturing or quality processes.</summary>
    ProcessRisk = 3,

    /// <summary>Gaps detected in document coverage, revision, or compliance.</summary>
    DocumentGap = 4,

    /// <summary>Risk identified from audit findings, repeated observations, or open items.</summary>
    AuditRisk = 5,

    /// <summary>Recommendation based on CAPA effectiveness, repeat defects, or closure rates.</summary>
    CAPARecommendation = 6,
}
