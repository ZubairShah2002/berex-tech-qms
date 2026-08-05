namespace BerexQms.Application.Training.DTOs;

public sealed record TrainingAssignmentDto(
    Guid Id,
    Guid EmployeeId,
    Guid CourseId,
    string? CourseName,
    Guid AssignedBy,
    DateTime AssignedDate,
    DateTime DueDate,
    string Status,
    CompletionDto? Completion,
    DateTime CreatedAt);

public sealed record CompletionDto(
    DateTime CompletionDate,
    decimal? Score,
    string Result,
    Guid? AssessorId,
    string? EvidenceRef);
