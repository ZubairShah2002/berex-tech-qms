namespace BerexQms.Application.Training.DTOs;

public sealed record TrainingCourseDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    decimal DurationHours,
    string? AssessmentType,
    string? PassCriteria,
    Guid? QualificationId,
    bool IsActive,
    DateTime CreatedAt);
