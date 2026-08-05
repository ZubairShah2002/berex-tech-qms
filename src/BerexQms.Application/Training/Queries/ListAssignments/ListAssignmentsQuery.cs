using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Training.DTOs;

namespace BerexQms.Application.Training.Queries.ListAssignments;

public sealed record ListAssignmentsQuery(
    Guid? EmployeeId,
    string? Status,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<TrainingAssignmentDto>>;
