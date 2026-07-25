using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.ProductCatalog;

public static class PartErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Part.NotFound", "The specified part was not found.");

    public static readonly Error PartNumberExists = Error.Conflict(
        "Part.PartNumberExists", "A part with this part number already exists.");

    public static readonly Error RevisionNotFound = Error.NotFound(
        "Part.RevisionNotFound", "The specified revision was not found.");

    public static readonly Error IsObsolete = Error.Validation(
        "Part.IsObsolete", "Cannot perform this operation on an obsolete part.");

    public static readonly Error ChildPartNotFound = Error.NotFound(
        "Part.ChildPartNotFound", "The specified child part was not found.");
}
