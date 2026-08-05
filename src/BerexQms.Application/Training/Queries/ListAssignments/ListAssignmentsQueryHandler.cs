using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Training.DTOs;
using BerexQms.Domain.Training.Entities;
using BerexQms.Domain.Training.Repositories;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Training.Queries.ListAssignments;

internal sealed class ListAssignmentsQueryHandler
    : IQueryHandler<ListAssignmentsQuery, PagedResult<TrainingAssignmentDto>>
{
    private readonly ITrainingAssignmentRepository _repository;

    public ListAssignmentsQueryHandler(ITrainingAssignmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<TrainingAssignmentDto>>> Handle(
        ListAssignmentsQuery request, CancellationToken cancellationToken)
    {
        var spec = new AssignmentListSpecification(
            request.EmployeeId, request.Status, request.Page, request.PageSize);

        var items = await _repository.ListAsync(spec, cancellationToken);

        var countSpec = new AssignmentCountSpecification(request.EmployeeId, request.Status);
        var totalCount = await _repository.CountAsync(countSpec, cancellationToken);

        var dtos = items.Select(a => new TrainingAssignmentDto(
            a.Id,
            a.EmployeeId,
            a.CourseId,
            null, // CourseName populated at API level if needed
            a.AssignedBy,
            a.AssignedDate,
            a.DueDate,
            a.Status,
            a.Completion is not null
                ? new CompletionDto(
                    a.Completion.CompletionDate,
                    a.Completion.Score,
                    a.Completion.Result,
                    a.Completion.AssessorId,
                    a.Completion.EvidenceRef)
                : null,
            a.CreatedAt)).ToList();

        return new PagedResult<TrainingAssignmentDto>(dtos, totalCount, request.Page, request.PageSize);
    }

    private sealed class AssignmentListSpecification : Specification<TrainingAssignment>
    {
        public AssignmentListSpecification(Guid? employeeId, string? status, int page, int pageSize)
        {
            ApplyFilters(employeeId, status);
            ApplyOrderByDescending(a => a.CreatedAt);
            ApplyPaging((page - 1) * pageSize, pageSize);
        }

        private void ApplyFilters(Guid? employeeId, string? status)
        {
            var hasEmployee = employeeId.HasValue;
            var hasStatus = !string.IsNullOrWhiteSpace(status);

            if (!hasEmployee && !hasStatus)
                return;

            ApplyCriteria(a =>
                (!hasEmployee || a.EmployeeId == employeeId) &&
                (!hasStatus || a.Status == status));
        }
    }

    private sealed class AssignmentCountSpecification : Specification<TrainingAssignment>
    {
        public AssignmentCountSpecification(Guid? employeeId, string? status)
        {
            var hasEmployee = employeeId.HasValue;
            var hasStatus = !string.IsNullOrWhiteSpace(status);

            if (!hasEmployee && !hasStatus)
                return;

            ApplyCriteria(a =>
                (!hasEmployee || a.EmployeeId == employeeId) &&
                (!hasStatus || a.Status == status));
        }
    }
}
