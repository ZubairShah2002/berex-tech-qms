using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Capa.DTOs;
using BerexQms.Domain.Capa.Entities;
using BerexQms.Domain.Capa.Enums;
using BerexQms.Domain.Capa.Repositories;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Capa.Queries.ListCapas;

public sealed class ListCapasQueryHandler
    : IQueryHandler<ListCapasQuery, PagedResult<CAPADto>>
{
    private readonly ICAPARepository _repository;

    public ListCapasQueryHandler(ICAPARepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<CAPADto>>> Handle(
        ListCapasQuery request, CancellationToken cancellationToken)
    {
        var spec = new CapaListSpecification(
            request.SearchTerm, request.Status, request.Priority,
            request.SourceType, request.Page, request.PageSize);

        var records = await _repository.ListAsync(spec, cancellationToken);

        var countSpec = new CapaCountSpecification(
            request.SearchTerm, request.Status, request.Priority, request.SourceType);
        var totalCount = await _repository.CountAsync(countSpec, cancellationToken);

        var dtos = records.Select(r => new CAPADto(
            r.Id,
            r.CapaNumber,
            r.Title,
            r.Status.ToString(),
            r.Priority.ToString(),
            r.Source.SourceType.ToString(),
            r.OwnerId,
            r.AssignedTo,
            r.SourceNonConformanceId,
            r.TargetClosureDate,
            r.Actions.Count,
            r.Actions.Count(a => a.CompletedAt is not null),
            r.CreatedAt)).ToList();

        return new PagedResult<CAPADto>(dtos, totalCount, request.Page, request.PageSize);
    }

    private sealed class CapaListSpecification : Specification<CAPARecord>
    {
        public CapaListSpecification(
            string? searchTerm, string? status, string? priority,
            string? sourceType, int page, int pageSize)
        {
            ApplyFilters(searchTerm, status, priority, sourceType);
            ApplyOrderByDescending(r => r.CreatedAt);
            ApplyPaging((page - 1) * pageSize, pageSize);
        }

        private void ApplyFilters(
            string? searchTerm, string? status, string? priority, string? sourceType)
        {
            var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);
            var hasStatus = !string.IsNullOrWhiteSpace(status) && Enum.TryParse<CAPAStatus>(status, true, out _);
            var hasPriority = !string.IsNullOrWhiteSpace(priority) && Enum.TryParse<CAPAPriority>(priority, true, out _);
            var hasSourceType = !string.IsNullOrWhiteSpace(sourceType) && Enum.TryParse<CAPASourceType>(sourceType, true, out _);

            if (!hasSearch && !hasStatus && !hasPriority && !hasSourceType)
                return;

            var term = searchTerm?.ToUpperInvariant() ?? string.Empty;
            var parsedStatus = hasStatus ? Enum.Parse<CAPAStatus>(status!, true) : default;
            var parsedPriority = hasPriority ? Enum.Parse<CAPAPriority>(priority!, true) : default;
            var parsedSourceType = hasSourceType ? Enum.Parse<CAPASourceType>(sourceType!, true) : default;

            ApplyCriteria(r =>
                (!hasSearch || r.CapaNumber.ToUpper().Contains(term) || r.Title.ToUpper().Contains(term)) &&
                (!hasStatus || r.Status == parsedStatus) &&
                (!hasPriority || r.Priority == parsedPriority) &&
                (!hasSourceType || r.Source.SourceType == parsedSourceType));
        }
    }

    private sealed class CapaCountSpecification : Specification<CAPARecord>
    {
        public CapaCountSpecification(
            string? searchTerm, string? status, string? priority, string? sourceType)
        {
            var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);
            var hasStatus = !string.IsNullOrWhiteSpace(status) && Enum.TryParse<CAPAStatus>(status, true, out _);
            var hasPriority = !string.IsNullOrWhiteSpace(priority) && Enum.TryParse<CAPAPriority>(priority, true, out _);
            var hasSourceType = !string.IsNullOrWhiteSpace(sourceType) && Enum.TryParse<CAPASourceType>(sourceType, true, out _);

            if (!hasSearch && !hasStatus && !hasPriority && !hasSourceType)
                return;

            var term = searchTerm?.ToUpperInvariant() ?? string.Empty;
            var parsedStatus = hasStatus ? Enum.Parse<CAPAStatus>(status!, true) : default;
            var parsedPriority = hasPriority ? Enum.Parse<CAPAPriority>(priority!, true) : default;
            var parsedSourceType = hasSourceType ? Enum.Parse<CAPASourceType>(sourceType!, true) : default;

            ApplyCriteria(r =>
                (!hasSearch || r.CapaNumber.ToUpper().Contains(term) || r.Title.ToUpper().Contains(term)) &&
                (!hasStatus || r.Status == parsedStatus) &&
                (!hasPriority || r.Priority == parsedPriority) &&
                (!hasSourceType || r.Source.SourceType == parsedSourceType));
        }
    }
}
