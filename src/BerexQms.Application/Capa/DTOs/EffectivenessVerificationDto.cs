namespace BerexQms.Application.Capa.DTOs;

public sealed record EffectivenessVerificationDto(
    Guid Id,
    DateTime ScheduledDate,
    string VerificationCriteria,
    string? VerifierId,
    string? Result,
    string? Evidence,
    bool? IsEffective,
    DateTime? VerifiedAt,
    DateTime CreatedAt);
