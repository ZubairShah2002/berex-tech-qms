namespace BerexQms.Domain.AiEngine.Enums;

/// <summary>
/// Classifies the type of knowledge captured in an AI context document.
/// Each type represents a specific domain of QMS knowledge.
/// </summary>
public enum AiContextType
{
    /// <summary>Product specifications, BOM, structure, and revision history.</summary>
    Product = 1,

    /// <summary>Inspection results, defects, reject data, and measurement trends.</summary>
    Quality = 2,

    /// <summary>Supplier performance, incoming inspection, reject trends, and SCARs.</summary>
    Supplier = 3,

    /// <summary>SOPs, work instructions, quality standards, and ISO documents.</summary>
    Document = 4,

    /// <summary>Non-conformance reports, root cause analysis, and disposition data.</summary>
    NonConformance = 5,

    /// <summary>CAPA records, corrective actions, effectiveness verification.</summary>
    CorrectiveAction = 6,

    /// <summary>Audit findings, observations, and corrective action tracking.</summary>
    Audit = 7,

    /// <summary>Calibration records, equipment status, and compliance data.</summary>
    Calibration = 8,

    /// <summary>Training records, competency assessments, and qualification status.</summary>
    Training = 9,

    /// <summary>SPC control charts, process capability, and trend analysis.</summary>
    StatisticalProcess = 10,
}
