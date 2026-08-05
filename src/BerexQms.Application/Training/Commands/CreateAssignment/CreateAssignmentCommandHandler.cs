using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.Training.Entities;
using BerexQms.Domain.Training.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Training.Commands.CreateAssignment;

internal sealed class CreateAssignmentCommandHandler
    : ICommandHandler<CreateAssignmentCommand, Guid>
{
    private readonly ITrainingAssignmentRepository _repository;
    private readonly ITrainingCourseRepository _courseRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateAssignmentCommandHandler(
        ITrainingAssignmentRepository repository,
        ITrainingCourseRepository courseRepository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _courseRepository = courseRepository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateAssignmentCommand request, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(request.CourseId, cancellationToken);
        if (course is null)
            return TrainingErrors.CourseNotFound;

        var assignment = TrainingAssignment.Create(
            Guid.NewGuid(),
            _tenantContext.CurrentTenantId,
            request.EmployeeId,
            request.CourseId,
            _currentUserService.UserId,
            DateTime.UtcNow,
            request.DueDate);

        await _repository.AddAsync(assignment, cancellationToken);

        return assignment.Id;
    }
}
