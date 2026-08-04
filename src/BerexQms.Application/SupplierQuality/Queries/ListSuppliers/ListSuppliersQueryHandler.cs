using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.SupplierQuality.DTOs;
using BerexQms.Domain.SupplierQuality.Entities;
using BerexQms.Domain.SupplierQuality.Repositories;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.SupplierQuality.Queries.ListSuppliers;

internal sealed class ListSuppliersQueryHandler
    : IQueryHandler<ListSuppliersQuery, PagedResult<SupplierDto>>
{
    private readonly ISupplierRepository _repository;

    public ListSuppliersQueryHandler(ISupplierRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<SupplierDto>>> Handle(
        ListSuppliersQuery request, CancellationToken cancellationToken)
    {
        var spec = new SupplierListSpecification(
            request.SearchTerm, request.Status, request.RiskLevel, request.Page, request.PageSize);

        var suppliers = await _repository.ListAsync(spec, cancellationToken);

        var countSpec = new SupplierCountSpecification(
            request.SearchTerm, request.Status, request.RiskLevel);
        var totalCount = await _repository.CountAsync(countSpec, cancellationToken);

        var dtos = suppliers.Select(s => new SupplierDto(
            s.Id,
            s.Code,
            s.Name,
            s.Status,
            s.RiskLevel,
            s.Tier,
            s.ApprovedSince,
            s.PrimaryContact?.Name,
            s.PrimaryContact?.Email,
            s.Approvals.Count,
            s.Scars.Count,
            s.CreatedAt)).ToList();

        return new PagedResult<SupplierDto>(dtos, totalCount, request.Page, request.PageSize);
    }

    private sealed class SupplierListSpecification : Specification<Supplier>
    {
        public SupplierListSpecification(
            string? searchTerm, string? status, string? riskLevel, int page, int pageSize)
        {
            ApplyFilters(searchTerm, status, riskLevel);
            ApplyOrderByDescending(s => s.CreatedAt);
            ApplyPaging((page - 1) * pageSize, pageSize);
            AddInclude(s => s.Approvals);
            AddInclude(s => s.Scars);
        }

        private void ApplyFilters(string? searchTerm, string? status, string? riskLevel)
        {
            var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);
            var hasStatus = !string.IsNullOrWhiteSpace(status);
            var hasRisk = !string.IsNullOrWhiteSpace(riskLevel);

            if (!hasSearch && !hasStatus && !hasRisk)
                return;

            var term = searchTerm?.ToUpperInvariant() ?? string.Empty;

            ApplyCriteria(s =>
                (!hasSearch || s.Code.ToUpper().Contains(term) || s.Name.ToUpper().Contains(term)) &&
                (!hasStatus || s.Status == status) &&
                (!hasRisk || s.RiskLevel == riskLevel));
        }
    }

    private sealed class SupplierCountSpecification : Specification<Supplier>
    {
        public SupplierCountSpecification(string? searchTerm, string? status, string? riskLevel)
        {
            var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);
            var hasStatus = !string.IsNullOrWhiteSpace(status);
            var hasRisk = !string.IsNullOrWhiteSpace(riskLevel);

            if (!hasSearch && !hasStatus && !hasRisk)
                return;

            var term = searchTerm?.ToUpperInvariant() ?? string.Empty;

            ApplyCriteria(s =>
                (!hasSearch || s.Code.ToUpper().Contains(term) || s.Name.ToUpper().Contains(term)) &&
                (!hasStatus || s.Status == status) &&
                (!hasRisk || s.RiskLevel == riskLevel));
        }
    }
}
