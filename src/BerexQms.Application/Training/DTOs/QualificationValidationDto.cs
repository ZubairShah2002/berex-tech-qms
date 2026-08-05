namespace BerexQms.Application.Training.DTOs;

public sealed record QualificationValidationDto(
    bool IsQualified,
    DateTime? ExpiryDate,
    string? QualificationCode);
