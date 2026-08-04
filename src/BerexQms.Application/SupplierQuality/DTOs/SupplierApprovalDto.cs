namespace BerexQms.Application.SupplierQuality.DTOs;

public sealed record SupplierApprovalDto(
    Guid Id,
    string ScopeDescription,
    DateTime ApprovedDate,
    DateTime? ExpiryDate,
    string? Conditions,
    bool IsActive);
