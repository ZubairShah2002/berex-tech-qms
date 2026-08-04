namespace BerexQms.Application.SupplierQuality.DTOs;

public sealed record ApprovedPartDto(
    Guid Id,
    Guid PartId,
    string? RevisionScope,
    DateTime ApprovalDate,
    bool IsActive);
