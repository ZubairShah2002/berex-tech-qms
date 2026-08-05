namespace BerexQms.Application.Training.DTOs;

public sealed record QualificationDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string? ScopeProductFamily,
    string? ScopeInspectionType,
    string? ScopeProcessArea,
    int ValidityMonths,
    int RenewalWindowDays,
    bool IsActive,
    DateTime CreatedAt);
