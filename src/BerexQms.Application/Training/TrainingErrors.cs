using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Training;

public static class TrainingErrors
{
    public static readonly Error QualificationNotFound = Error.NotFound(
        "Training.QualificationNotFound", "Qualification not found.");

    public static readonly Error QualificationCodeExists = Error.Conflict(
        "Training.QualificationCodeExists", "A qualification with this code already exists.");

    public static readonly Error CourseNotFound = Error.NotFound(
        "Training.CourseNotFound", "Training course not found.");

    public static readonly Error CourseCodeExists = Error.Conflict(
        "Training.CourseCodeExists", "A course with this code already exists.");

    public static readonly Error AssignmentNotFound = Error.NotFound(
        "Training.AssignmentNotFound", "Training assignment not found.");

    public static readonly Error CompetencyNotFound = Error.NotFound(
        "Training.CompetencyNotFound", "Competency record not found.");

    public static readonly Error InvalidResult = Error.Validation(
        "Training.InvalidResult", "Invalid assessment result. Use Pass or Fail.");

    public static readonly Error InvalidAction = Error.Validation(
        "Training.InvalidAction", "Invalid action.");
}
