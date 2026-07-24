namespace BerexQms.Domain.Common.Enums;

/// <summary>
/// Categorizes inspections by their position in the manufacturing workflow.
/// Each type has distinct sampling rules, checklist templates, and disposition authority.
/// </summary>
public enum InspectionType
{
    /// <summary>
    /// Incoming Quality Control. Performed on raw materials, components,
    /// or sub-assemblies received from suppliers before they enter production.
    /// Results feed supplier scorecards and may trigger Supplier Corrective Action Requests (SCARs).
    /// </summary>
    IQC = 0,

    /// <summary>
    /// In-Process Quality Control. Performed during manufacturing operations
    /// at defined control points. Detects process drift early and supports
    /// Statistical Process Control (SPC) data collection.
    /// </summary>
    IPQC = 1,

    /// <summary>
    /// Outgoing Quality Control. Final inspection before shipment to the customer.
    /// Verifies that finished goods meet all specification requirements
    /// and that packaging and labeling are correct.
    /// </summary>
    OQC = 2
}
