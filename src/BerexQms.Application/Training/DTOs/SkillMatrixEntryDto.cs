namespace BerexQms.Application.Training.DTOs;

public sealed record SkillMatrixEntryDto(
    Guid EmployeeId,
    Guid QualificationId,
    string QualificationCode,
    string QualificationName,
    string Status,
    DateTime? ExpiryDate);
