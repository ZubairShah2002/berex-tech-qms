using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Training.DTOs;
using BerexQms.Domain.Training.Entities;
using BerexQms.Domain.Training.Repositories;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Training.Queries.ListCourses;

internal sealed class ListCoursesQueryHandler
    : IQueryHandler<ListCoursesQuery, PagedResult<TrainingCourseDto>>
{
    private readonly ITrainingCourseRepository _repository;

    public ListCoursesQueryHandler(ITrainingCourseRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<TrainingCourseDto>>> Handle(
        ListCoursesQuery request, CancellationToken cancellationToken)
    {
        var spec = new CourseListSpecification(
            request.SearchTerm, request.QualificationId, request.Page, request.PageSize);

        var items = await _repository.ListAsync(spec, cancellationToken);

        var countSpec = new CourseCountSpecification(request.SearchTerm, request.QualificationId);
        var totalCount = await _repository.CountAsync(countSpec, cancellationToken);

        var dtos = items.Select(c => new TrainingCourseDto(
            c.Id,
            c.Code,
            c.Name,
            c.Description,
            c.DurationHours,
            c.AssessmentType,
            c.PassCriteria,
            c.QualificationId,
            c.IsActive,
            c.CreatedAt)).ToList();

        return new PagedResult<TrainingCourseDto>(dtos, totalCount, request.Page, request.PageSize);
    }

    private sealed class CourseListSpecification : Specification<TrainingCourse>
    {
        public CourseListSpecification(string? searchTerm, Guid? qualificationId, int page, int pageSize)
        {
            ApplyFilters(searchTerm, qualificationId);
            ApplyOrderByDescending(c => c.CreatedAt);
            ApplyPaging((page - 1) * pageSize, pageSize);
        }

        private void ApplyFilters(string? searchTerm, Guid? qualificationId)
        {
            var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);
            var hasQualification = qualificationId.HasValue;

            if (!hasSearch && !hasQualification)
                return;

            var term = searchTerm?.ToUpperInvariant() ?? string.Empty;

            ApplyCriteria(c =>
                (!hasSearch || c.Code.ToUpper().Contains(term) || c.Name.ToUpper().Contains(term)) &&
                (!hasQualification || c.QualificationId == qualificationId));
        }
    }

    private sealed class CourseCountSpecification : Specification<TrainingCourse>
    {
        public CourseCountSpecification(string? searchTerm, Guid? qualificationId)
        {
            var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);
            var hasQualification = qualificationId.HasValue;

            if (!hasSearch && !hasQualification)
                return;

            var term = searchTerm?.ToUpperInvariant() ?? string.Empty;

            ApplyCriteria(c =>
                (!hasSearch || c.Code.ToUpper().Contains(term) || c.Name.ToUpper().Contains(term)) &&
                (!hasQualification || c.QualificationId == qualificationId));
        }
    }
}
