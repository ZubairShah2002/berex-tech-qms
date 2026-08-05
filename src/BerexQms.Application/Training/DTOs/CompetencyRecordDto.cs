namespace BerexQms.Application.Training.DTOs;

public sealed record CompetencyRecordDto(
    Guid Id,
    Guid EmployeeId,
    Guid QualificationId,
    string Status,
    DateTime? QualifiedDate,
    DateTime? ExpiryDate,
    Guid? AssessorId,
    string? EvidenceRef);
