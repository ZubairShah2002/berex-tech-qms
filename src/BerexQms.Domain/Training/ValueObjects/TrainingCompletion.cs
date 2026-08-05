namespace BerexQms.Domain.Training.ValueObjects;

public sealed record TrainingCompletion(
    DateTime CompletionDate,
    decimal? Score,
    string Result,
    Guid? AssessorId,
    string? EvidenceRef);
