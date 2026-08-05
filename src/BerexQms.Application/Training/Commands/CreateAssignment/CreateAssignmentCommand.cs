using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Training.Commands.CreateAssignment;

public sealed record CreateAssignmentCommand(
    Guid EmployeeId,
    Guid CourseId,
    DateTime DueDate) : ICommand<Guid>;
