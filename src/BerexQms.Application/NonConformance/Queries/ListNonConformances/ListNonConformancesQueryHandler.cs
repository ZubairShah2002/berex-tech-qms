using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.NonConformance.DTOs;
using BerexQms.Domain.NonConformance.Entities;
using BerexQms.Domain.NonConformance.Enums;
using BerexQms.Domain.NonConformance.Repositories;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.NonConformance.Queries.ListNonConformances;

public sealed class ListNonConformancesQueryHandler
    : IQueryHandler<ListNonConformancesQuery, PagedResult<NonConformanceDto>>
{
    private readonly INonConformanceRepository _repository;

    public ListNonConformancesQueryHandler(INonConformanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<NonConformanceDto>>> Handle(
        ListNonConformancesQuery request, CancellationToken cancellationToken)
    {
        var spec = new NcListSpecification(
            request.SearchTerm, request.Status, request.Severity,
            request.Source, request.PartId, request.SupplierId,
            request.Page, request.PageSize);

        var records = await _repository.ListAsync(spec, cancellationToken);

        var countSpec = new NcCountSpecification(
            request.SearchTerm, request.Status, request.Severity,
            request.Source, request.PartId, request.SupplierId);
        var totalCount = await _repository.CountAsync(countSpec, cancellationToken);

        var dtos = records.Select(r => new NonConformanceDto(
            r.Id,
            r.NcrNumber,
            r.Status.ToString(),
            r.Severity.ToString(),
            r.Source.ToString(),
            r.DetectionPoint.ToString(),
            r.PartId,
            r.LotNumber,
            r.SupplierId,
            r.QuantityAffected,
            r.QuantityDefective,
            r.AssignedTo,
            r.CreatedAt)).ToList();

        return new PagedResult<NonConformanceDto>(dtos, totalCount, request.Page, request.PageSize);
    }

    private sealed class NcListSpecification : Specification<NonConformanceRecord>
    {
        public NcListSpecification(
            string? searchTerm, string? status, string? severity,
            string? source, Guid? partId, Guid? supplierId,
            int page, int pageSize)
        {
            ApplyFilters(searchTerm, status, severity, source, partId, supplierId);
            ApplyOrderByDescending(r => r.CreatedAt);
            ApplyPaging((page - 1) * pageSize, pageSize);
        }

        private void ApplyFilters(
            string? searchTerm, string? status, string? severity,
            string? source, Guid? partId, Guid? supplierId)
        {
            var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);
            var hasStatus = !string.IsNullOrWhiteSpace(status) && Enum.TryParse<NCStatus>(status, true, out _);
            var hasSeverity = !string.IsNullOrWhiteSpace(severity) && Enum.TryParse<NCSeverity>(severity, true, out _);
            var hasSource = !string.IsNullOrWhiteSpace(source) && Enum.TryParse<NCSource>(source, true, out _);
            var hasPart = partId.HasValue;
            var hasSupplier = supplierId.HasValue;

            if (!hasSearch && !hasStatus && !hasSeverity && !hasSource && !hasPart && !hasSupplier)
                return;

            var term = searchTerm?.ToUpperInvariant() ?? string.Empty;
            var parsedStatus = hasStatus ? Enum.Parse<NCStatus>(status!, true) : default;
            var parsedSeverity = hasSeverity ? Enum.Parse<NCSeverity>(severity!, true) : default;
            var parsedSource = hasSource ? Enum.Parse<NCSource>(source!, true) : default;
            var pid = partId ?? Guid.Empty;
            var sid = supplierId ?? Guid.Empty;

            ApplyCriteria(r =>
                (!hasSearch || r.NcrNumber.ToUpper().Contains(term) || r.Description.ToUpper().Contains(term)) &&
                (!hasStatus || r.Status == parsedStatus) &&
                (!hasSeverity || r.Severity == parsedSeverity) &&
                (!hasSource || r.Source == parsedSource) &&
                (!hasPart || r.PartId == pid) &&
                (!hasSupplier || r.SupplierId == sid));
        }
    }

    private sealed class NcCountSpecification : Specification<NonConformanceRecord>
    {
        public NcCountSpecification(
            string? searchTerm, string? status, string? severity,
            string? source, Guid? partId, Guid? supplierId)
        {
            var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);
            var hasStatus = !string.IsNullOrWhiteSpace(status) && Enum.TryParse<NCStatus>(status, true, out _);
            var hasSeverity = !string.IsNullOrWhiteSpace(severity) && Enum.TryParse<NCSeverity>(severity, true, out _);
            var hasSource = !string.IsNullOrWhiteSpace(source) && Enum.TryParse<NCSource>(source, true, out _);
            var hasPart = partId.HasValue;
            var hasSupplier = supplierId.HasValue;

            if (!hasSearch && !hasStatus && !hasSeverity && !hasSource && !hasPart && !hasSupplier)
                return;

            var term = searchTerm?.ToUpperInvariant() ?? string.Empty;
            var parsedStatus = hasStatus ? Enum.Parse<NCStatus>(status!, true) : default;
            var parsedSeverity = hasSeverity ? Enum.Parse<NCSeverity>(severity!, true) : default;
            var parsedSource = hasSource ? Enum.Parse<NCSource>(source!, true) : default;
            var pid = partId ?? Guid.Empty;
            var sid = supplierId ?? Guid.Empty;

            ApplyCriteria(r =>
                (!hasSearch || r.NcrNumber.ToUpper().Contains(term) || r.Description.ToUpper().Contains(term)) &&
                (!hasStatus || r.Status == parsedStatus) &&
                (!hasSeverity || r.Severity == parsedSeverity) &&
                (!hasSource || r.Source == parsedSource) &&
                (!hasPart || r.PartId == pid) &&
                (!hasSupplier || r.SupplierId == sid));
        }
    }
}
