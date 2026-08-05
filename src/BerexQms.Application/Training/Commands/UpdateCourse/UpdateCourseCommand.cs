using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Training.Commands.UpdateCourse;

public sealed record UpdateCourseCommand(
    Guid CourseId,
    string Name,
    string? Description,
    decimal DurationHours,
    string? AssessmentType,
    string? PassCriteria,
    Guid? QualificationId) : ICommand;
