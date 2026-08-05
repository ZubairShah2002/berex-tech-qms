using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.Training.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Training.Commands.UpdateCourse;

internal sealed class UpdateCourseCommandHandler
    : ICommandHandler<UpdateCourseCommand>
{
    private readonly ITrainingCourseRepository _repository;

    public UpdateCourseCommandHandler(ITrainingCourseRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _repository.GetByIdAsync(request.CourseId, cancellationToken);
        if (course is null)
            return Result.Failure(TrainingErrors.CourseNotFound);

        course.Update(
            request.Name,
            request.Description,
            request.DurationHours,
            request.AssessmentType,
            request.PassCriteria,
            request.QualificationId);

        return Result.Success();
    }
}
