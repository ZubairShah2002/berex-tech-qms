namespace BerexQms.Application.SupplierQuality.DTOs;

public sealed record SupplierDto(
    Guid Id,
    string Code,
    string Name,
    string Status,
    string RiskLevel,
    string? Tier,
    DateTime? ApprovedSince,
    string? ContactName,
    string? ContactEmail,
    int ApprovalCount,
    int ScarCount,
    DateTime CreatedAt);
