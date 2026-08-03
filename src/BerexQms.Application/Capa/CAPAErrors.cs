using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Capa;

public static class CAPAErrors
{
    public static readonly Error NotFound = Error.NotFound("CAPA.NotFound", "CAPA record not found.");
    public static readonly Error CapaNumberExists = Error.Conflict("CAPA.NumberExists", "A CAPA with this number already exists.");
    public static readonly Error ActionNotFound = Error.NotFound("CAPA.ActionNotFound", "CAPA action not found.");
    public static readonly Error VerificationNotFound = Error.NotFound("CAPA.VerificationNotFound", "Effectiveness verification not found.");
}
