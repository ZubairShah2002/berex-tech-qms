using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Training.DTOs;
using BerexQms.Domain.Training.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Training.Queries.GetAssignment;

internal sealed class GetAssignmentQueryHandler
    : IQueryHandler<GetAssignmentQuery, TrainingAssignmentDto>
{
    private readonly ITrainingAssignmentRepository _repository;

    public GetAssignmentQueryHandler(ITrainingAssignmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TrainingAssignmentDto>> Handle(
        GetAssignmentQuery request, CancellationToken cancellationToken)
    {
        var assignment = await _repository.GetWithCompletionAsync(request.AssignmentId, cancellationToken);
        if (assignment is null)
            return TrainingErrors.AssignmentNotFound;

        return new TrainingAssignmentDto(
            assignment.Id,
            assignment.EmployeeId,
            assignment.CourseId,
            null,
            assignment.AssignedBy,
            assignment.AssignedDate,
            assignment.DueDate,
            assignment.Status,
            assignment.Completion is not null
                ? new CompletionDto(
                    assignment.Completion.CompletionDate,
                    assignment.Completion.Score,
                    assignment.Completion.Result,
                    assignment.Completion.AssessorId,
                    assignment.Completion.EvidenceRef)
                : null,
            assignment.CreatedAt);
    }
}
