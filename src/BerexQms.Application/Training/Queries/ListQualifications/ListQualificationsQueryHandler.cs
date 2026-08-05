using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Training.DTOs;
using BerexQms.Domain.Training.Entities;
using BerexQms.Domain.Training.Repositories;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Training.Queries.ListQualifications;

internal sealed class ListQualificationsQueryHandler
    : IQueryHandler<ListQualificationsQuery, PagedResult<QualificationDto>>
{
    private readonly IQualificationRepository _repository;

    public ListQualificationsQueryHandler(IQualificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<QualificationDto>>> Handle(
        ListQualificationsQuery request, CancellationToken cancellationToken)
    {
        var spec = new QualificationListSpecification(
            request.SearchTerm, request.IsActive, request.Page, request.PageSize);

        var items = await _repository.ListAsync(spec, cancellationToken);

        var countSpec = new QualificationCountSpecification(request.SearchTerm, request.IsActive);
        var totalCount = await _repository.CountAsync(countSpec, cancellationToken);

        var dtos = items.Select(q => new QualificationDto(
            q.Id,
            q.Code,
            q.Name,
            q.Description,
            q.ScopeProductFamily,
            q.ScopeInspectionType,
            q.ScopeProcessArea,
            q.ValidityMonths,
            q.RenewalWindowDays,
            q.IsActive,
            q.CreatedAt)).ToList();

        return new PagedResult<QualificationDto>(dtos, totalCount, request.Page, request.PageSize);
    }

    private sealed class QualificationListSpecification : Specification<Qualification>
    {
        public QualificationListSpecification(string? searchTerm, bool? isActive, int page, int pageSize)
        {
            ApplyFilters(searchTerm, isActive);
            ApplyOrderByDescending(q => q.CreatedAt);
            ApplyPaging((page - 1) * pageSize, pageSize);
        }

        private void ApplyFilters(string? searchTerm, bool? isActive)
        {
            var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);

            if (!hasSearch && !isActive.HasValue)
                return;

            var term = searchTerm?.ToUpperInvariant() ?? string.Empty;

            ApplyCriteria(q =>
                (!hasSearch || q.Code.ToUpper().Contains(term) || q.Name.ToUpper().Contains(term)) &&
                (!isActive.HasValue || q.IsActive == isActive.Value));
        }
    }

    private sealed class QualificationCountSpecification : Specification<Qualification>
    {
        public QualificationCountSpecification(string? searchTerm, bool? isActive)
        {
            var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);

            if (!hasSearch && !isActive.HasValue)
                return;

            var term = searchTerm?.ToUpperInvariant() ?? string.Empty;

            ApplyCriteria(q =>
                (!hasSearch || q.Code.ToUpper().Contains(term) || q.Name.ToUpper().Contains(term)) &&
                (!isActive.HasValue || q.IsActive == isActive.Value));
        }
    }
}
