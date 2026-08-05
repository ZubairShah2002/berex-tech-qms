using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.Training.Entities;
using BerexQms.Domain.Training.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Training.Commands.CreateCourse;

internal sealed class CreateCourseCommandHandler
    : ICommandHandler<CreateCourseCommand, Guid>
{
    private readonly ITrainingCourseRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CreateCourseCommandHandler(ITrainingCourseRepository repository, ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.CodeExistsAsync(request.Code, cancellationToken))
            return TrainingErrors.CourseCodeExists;

        var course = TrainingCourse.Create(
            Guid.NewGuid(),
            _tenantContext.CurrentTenantId,
            request.Code,
            request.Name,
            request.Description,
            request.DurationHours,
            request.AssessmentType,
            request.PassCriteria,
            request.QualificationId);

        await _repository.AddAsync(course, cancellationToken);

        return course.Id;
    }
}
