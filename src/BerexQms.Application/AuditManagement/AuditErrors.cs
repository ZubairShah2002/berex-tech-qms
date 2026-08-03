using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AuditManagement;

public static class AuditErrors
{
    public static readonly Error NotFound = Error.NotFound("Audit.NotFound", "Audit plan not found.");
    public static readonly Error AlreadyExists = Error.Conflict("Audit.PlanNameExists", "An audit plan with this name and year already exists.");
    public static readonly Error AuditNotFound = Error.NotFound("Audit.AuditNotFound", "Audit record not found.");
    public static readonly Error FindingNotFound = Error.NotFound("Audit.FindingNotFound", "Audit finding not found.");
    public static readonly Error InvalidStatus = Error.Validation("Audit.InvalidStatus", "Invalid audit status or value.");
}
