using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.NonConformance;

public static class NonConformanceErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "NonConformance.NotFound", "The specified non-conformance was not found.");

    public static readonly Error NcrNumberExists = Error.Conflict(
        "NonConformance.NumberExists", "A non-conformance with this number already exists.");

    public static readonly Error InvalidSeverity = Error.Validation(
        "NonConformance.InvalidSeverity", "Invalid severity. Valid values: Minor, Major, Critical.");

    public static readonly Error InvalidSource = Error.Validation(
        "NonConformance.InvalidSource", "Invalid source. Valid values: Inspection, LineFinding, CustomerComplaint, AuditFinding, SupplierNotification.");

    public static readonly Error InvalidDetectionPoint = Error.Validation(
        "NonConformance.InvalidDetectionPoint", "Invalid detection point.");

    public static readonly Error InvalidDispositionType = Error.Validation(
        "NonConformance.InvalidDispositionType", "Invalid disposition type. Valid values: UseAsIs, Rework, Scrap, ReturnToSupplier.");

    public static readonly Error ContainmentActionNotFound = Error.NotFound(
        "NonConformance.ContainmentNotFound", "The specified containment action was not found.");

    public static readonly Error PartNotFound = Error.NotFound(
        "NonConformance.PartNotFound", "The specified part was not found.");
}
