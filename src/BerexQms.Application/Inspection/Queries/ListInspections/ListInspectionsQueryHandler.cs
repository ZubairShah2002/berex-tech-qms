using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Inspection.DTOs;
using BerexQms.Domain.Inspection.Entities;
using BerexQms.Domain.Inspection.Enums;
using BerexQms.Domain.Inspection.Repositories;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Inspection.Queries.ListInspections;

public sealed class ListInspectionsQueryHandler
    : IQueryHandler<ListInspectionsQuery, PagedResult<InspectionDto>>
{
    private readonly IInspectionRepository _inspectionRepository;

    public ListInspectionsQueryHandler(IInspectionRepository inspectionRepository)
    {
        _inspectionRepository = inspectionRepository;
    }

    public async Task<Result<PagedResult<InspectionDto>>> Handle(
        ListInspectionsQuery request, CancellationToken cancellationToken)
    {
        var spec = new InspectionListSpecification(
            request.SearchTerm, request.Type, request.Status,
            request.PartId, request.Page, request.PageSize);

        var records = await _inspectionRepository.ListAsync(spec, cancellationToken);

        var countSpec = new InspectionCountSpecification(
            request.SearchTerm, request.Type, request.Status, request.PartId);
        var totalCount = await _inspectionRepository.CountAsync(countSpec, cancellationToken);

        var dtos = records.Select(r => new InspectionDto(
            r.Id,
            r.InspectionNumber,
            r.Type.ToString(),
            r.Status.ToString(),
            r.PartId,
            r.PartRevisionId,
            r.LotNumber,
            r.LotSize,
            r.SampleSize,
            r.SupplierId,
            r.InspectorId,
            r.Result?.ToString(),
            r.CreatedAt)).ToList();

        return new PagedResult<InspectionDto>(dtos, totalCount, request.Page, request.PageSize);
    }

    private sealed class InspectionListSpecification : Specification<InspectionRecord>
    {
        public InspectionListSpecification(
            string? searchTerm, string? type, string? status,
            Guid? partId, int page, int pageSize)
        {
            ApplyFilters(searchTerm, type, status, partId);
            ApplyOrderByDescending(r => r.CreatedAt);
            ApplyPaging((page - 1) * pageSize, pageSize);
        }

        private void ApplyFilters(string? searchTerm, string? type, string? status, Guid? partId)
        {
            var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);
            var hasType = !string.IsNullOrWhiteSpace(type) && Enum.TryParse<InspectionType>(type, true, out _);
            var hasStatus = !string.IsNullOrWhiteSpace(status) && Enum.TryParse<InspectionStatus>(status, true, out _);
            var hasPart = partId.HasValue;

            if (!hasSearch && !hasType && !hasStatus && !hasPart)
                return;

            var term = searchTerm?.ToUpperInvariant() ?? string.Empty;
            var parsedType = hasType ? Enum.Parse<InspectionType>(type!, true) : default;
            var parsedStatus = hasStatus ? Enum.Parse<InspectionStatus>(status!, true) : default;
            var pid = partId ?? Guid.Empty;

            ApplyCriteria(r =>
                (!hasSearch || r.InspectionNumber.Contains(term)) &&
                (!hasType || r.Type == parsedType) &&
                (!hasStatus || r.Status == parsedStatus) &&
                (!hasPart || r.PartId == pid));
        }
    }

    private sealed class InspectionCountSpecification : Specification<InspectionRecord>
    {
        public InspectionCountSpecification(
            string? searchTerm, string? type, string? status, Guid? partId)
        {
            var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);
            var hasType = !string.IsNullOrWhiteSpace(type) && Enum.TryParse<InspectionType>(type, true, out _);
            var hasStatus = !string.IsNullOrWhiteSpace(status) && Enum.TryParse<InspectionStatus>(status, true, out _);
            var hasPart = partId.HasValue;

            if (!hasSearch && !hasType && !hasStatus && !hasPart)
                return;

            var term = searchTerm?.ToUpperInvariant() ?? string.Empty;
            var parsedType = hasType ? Enum.Parse<InspectionType>(type!, true) : default;
            var parsedStatus = hasStatus ? Enum.Parse<InspectionStatus>(status!, true) : default;
            var pid = partId ?? Guid.Empty;

            ApplyCriteria(r =>
                (!hasSearch || r.InspectionNumber.Contains(term)) &&
                (!hasType || r.Type == parsedType) &&
                (!hasStatus || r.Status == parsedStatus) &&
                (!hasPart || r.PartId == pid));
        }
    }
}
