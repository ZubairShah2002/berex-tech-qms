using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Training.DTOs;

namespace BerexQms.Application.Training.Queries.ListCourses;

public sealed record ListCoursesQuery(
    string? SearchTerm,
    Guid? QualificationId,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<TrainingCourseDto>>;
