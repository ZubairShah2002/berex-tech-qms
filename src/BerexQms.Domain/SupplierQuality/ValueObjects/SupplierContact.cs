namespace BerexQms.Domain.SupplierQuality.ValueObjects;

public sealed record SupplierContact(
    string Name,
    string Role,
    string Email,
    string? Phone);
