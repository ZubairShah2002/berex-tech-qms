namespace BerexQms.Application.Spc.DTOs;

public sealed record DataPointDto(
    Guid Id,
    decimal Value,
    string? SubgroupValues,
    int SampleSize,
    DateTime Timestamp,
    Guid? InspectionId,
    string? RuleViolation,
    bool IsOutOfControl);
