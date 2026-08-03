using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.DocumentControl;

public static class DocumentErrors
{
    public static readonly Error NotFound = Error.NotFound("Document.NotFound", "Document not found.");
    public static readonly Error DocumentNumberExists = Error.Conflict("Document.NumberExists", "A document with this number already exists.");
    public static readonly Error VersionNotFound = Error.NotFound("Document.VersionNotFound", "Document version not found.");
    public static readonly Error WorkflowNotFound = Error.NotFound("Document.WorkflowNotFound", "Approval workflow not found.");
    public static readonly Error DistributionNotFound = Error.NotFound("Document.DistributionNotFound", "Distribution record not found.");
}
