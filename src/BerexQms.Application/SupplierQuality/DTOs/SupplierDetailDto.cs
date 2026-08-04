namespace BerexQms.Application.SupplierQuality.DTOs;

public sealed record SupplierDetailDto(
    Guid Id,
    string Code,
    string Name,
    string Status,
    string RiskLevel,
    string? Tier,
    DateTime? ApprovedSince,
    string? ContactName,
    string? ContactRole,
    string? ContactEmail,
    string? ContactPhone,
    string? RiskAssessmentLevel,
    string? RiskAssessmentFactors,
    DateTime? RiskAssessedAt,
    IReadOnlyList<SupplierApprovalDto> Approvals,
    IReadOnlyList<SupplierScorecardDto> Scorecards,
    IReadOnlyList<SCARRecordDto> Scars,
    IReadOnlyList<ApprovedPartDto> ApprovedParts,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? ModifiedAt);
