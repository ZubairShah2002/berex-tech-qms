using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Training.Commands.ManageCompetency;

public sealed record ManageCompetencyCommand(
    Guid EmployeeId,
    Guid QualificationId,
    string Action,
    DateTime? QualifiedDate,
    Guid? AssessorId,
    string? EvidenceRef) : ICommand;
