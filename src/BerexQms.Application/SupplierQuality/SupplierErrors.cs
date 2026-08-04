using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.SupplierQuality;

public static class SupplierErrors
{
    public static readonly Error NotFound = Error.NotFound("Supplier.NotFound", "Supplier not found.");
    public static readonly Error CodeExists = Error.Conflict("Supplier.CodeExists", "A supplier with this code already exists.");
    public static readonly Error ScarNotFound = Error.NotFound("Supplier.ScarNotFound", "SCAR record not found.");
    public static readonly Error InvalidStatus = Error.Validation("Supplier.InvalidStatus", "Invalid status or value.");
}
