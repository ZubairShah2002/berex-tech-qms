namespace BerexQms.Domain.Common.Enums;

/// <summary>
/// Defines the possible disposition decisions for a lot or item that has been inspected.
/// Each disposition carries specific downstream requirements such as approvals,
/// documentation, and material handling actions.
/// </summary>
public enum DispositionType
{
    /// <summary>
    /// The lot or item meets all specification requirements and is released for use or shipment.
    /// </summary>
    Accept = 0,

    /// <summary>
    /// The lot or item deviates from specification but is deemed fit for use under specific conditions.
    /// Creates a Deviation record and requires Manager-level electronic signature approval.
    /// </summary>
    AcceptWithDeviation = 1,

    /// <summary>
    /// The lot requires 100% inspection to separate conforming from non-conforming items.
    /// Conforming items are accepted; non-conforming items are re-dispositioned individually.
    /// </summary>
    Sort = 2,

    /// <summary>
    /// The lot or item can be brought into conformance through a defined rework process.
    /// Requires a rework instruction and re-inspection after rework is completed.
    /// </summary>
    Rework = 3,

    /// <summary>
    /// The lot or item is returned to the supplier for credit, replacement, or corrective action.
    /// Triggers a Supplier Corrective Action Request (SCAR) and updates the supplier scorecard.
    /// </summary>
    ReturnToSupplier = 4,

    /// <summary>
    /// The lot or item is permanently rejected and destroyed or recycled.
    /// Requires documentation of the scrap quantity for cost-of-quality tracking.
    /// </summary>
    Scrap = 5
}
