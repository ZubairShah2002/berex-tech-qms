using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Training.Commands.CreateCourse;

public sealed record CreateCourseCommand(
    string Code,
    string Name,
    string? Description,
    decimal DurationHours,
    string? AssessmentType,
    string? PassCriteria,
    Guid? QualificationId) : ICommand<Guid>;
