using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.Training.Enums;
using BerexQms.Domain.Training.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Training.Commands.CompleteAssignment;

internal sealed class CompleteAssignmentCommandHandler
    : ICommandHandler<CompleteAssignmentCommand>
{
    private readonly ITrainingAssignmentRepository _assignmentRepository;
    private readonly ITrainingCourseRepository _courseRepository;
    private readonly IQualificationRepository _qualificationRepository;

    public CompleteAssignmentCommandHandler(
        ITrainingAssignmentRepository assignmentRepository,
        ITrainingCourseRepository courseRepository,
        IQualificationRepository qualificationRepository)
    {
        _assignmentRepository = assignmentRepository;
        _courseRepository = courseRepository;
        _qualificationRepository = qualificationRepository;
    }

    public async Task<Result> Handle(CompleteAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(request.AssignmentId, cancellationToken);
        if (assignment is null)
            return Result.Failure(TrainingErrors.AssignmentNotFound);

        if (!Enum.TryParse<AssessmentResult>(request.Result, true, out var assessmentResult))
            return Result.Failure(TrainingErrors.InvalidResult);

        // Look up course to get linked qualification and validity
        Guid? qualificationId = null;
        int? validityMonths = null;

        var course = await _courseRepository.GetWithQualificationAsync(assignment.CourseId, cancellationToken);
        if (course?.QualificationId is not null)
        {
            qualificationId = course.QualificationId;
            var qualification = await _qualificationRepository.GetByIdAsync(
                course.QualificationId.Value, cancellationToken);
            validityMonths = qualification?.ValidityMonths;
        }

        assignment.Complete(
            request.CompletionDate,
            request.Score,
            assessmentResult,
            request.AssessorId,
            request.EvidenceRef,
            qualificationId,
            validityMonths);

        return Result.Success();
    }
}
