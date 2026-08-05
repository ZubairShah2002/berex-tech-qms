using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Training.Commands.CompleteAssignment;

public sealed record CompleteAssignmentCommand(
    Guid AssignmentId,
    DateTime CompletionDate,
    decimal? Score,
    string Result,
    Guid? AssessorId,
    string? EvidenceRef) : ICommand;
