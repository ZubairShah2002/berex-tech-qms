namespace BerexQms.Application.NonConformance.DTOs;

public sealed record ContainmentActionDto(
    Guid Id,
    string Description,
    string ActionTakenBy,
    DateTime ActionTakenAt,
    bool IsVerified,
    string? VerifiedBy,
    DateTime? VerifiedAt);
