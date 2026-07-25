using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.ProductCatalog.DTOs;
using BerexQms.Domain.ProductCatalog.Entities;
using BerexQms.Domain.ProductCatalog.Enums;
using BerexQms.Domain.ProductCatalog.Repositories;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.ProductCatalog.Queries.ListParts;

public sealed class ListPartsQueryHandler : IQueryHandler<ListPartsQuery, PagedResult<PartDto>>
{
    private readonly IPartRepository _partRepository;

    public ListPartsQueryHandler(IPartRepository partRepository)
    {
        _partRepository = partRepository;
    }

    public async Task<Result<PagedResult<PartDto>>> Handle(ListPartsQuery request, CancellationToken cancellationToken)
    {
        var spec = new PartListSpecification(
            request.SearchTerm, request.Status, request.ProductFamily, request.Category,
            request.Page, request.PageSize);

        var parts = await _partRepository.ListAsync(spec, cancellationToken);

        var countSpec = new PartCountSpecification(
            request.SearchTerm, request.Status, request.ProductFamily, request.Category);
        var totalCount = await _partRepository.CountAsync(countSpec, cancellationToken);

        var dtos = parts.Select(p =>
        {
            var currentRevision = p.Revisions
                .FirstOrDefault(r => r.Status == RevisionStatus.Released);

            return new PartDto(
                p.Id,
                p.PartNumber,
                p.Name,
                p.Description,
                p.ProductFamily,
                p.Category,
                p.SerializationMode.ToString(),
                p.Status.ToString(),
                p.UnitOfMeasure,
                currentRevision?.RevisionCode,
                p.Revisions.Count,
                p.CreatedAt);
        }).ToList();

        return new PagedResult<PartDto>(dtos, totalCount, request.Page, request.PageSize);
    }

    private sealed class PartListSpecification : Specification<Part>
    {
        public PartListSpecification(string? searchTerm, string? status, string? productFamily, string? category, int page, int pageSize)
        {
            ApplyFilters(searchTerm, status, productFamily, category);
            AddInclude(p => p.Revisions);
            ApplyOrderBy(p => p.PartNumber);
            ApplyPaging((page - 1) * pageSize, pageSize);
        }

        private void ApplyFilters(string? searchTerm, string? status, string? productFamily, string? category)
        {
            var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);
            var hasStatus = !string.IsNullOrWhiteSpace(status) && Enum.TryParse<PartStatus>(status, true, out _);
            var hasFamily = !string.IsNullOrWhiteSpace(productFamily);
            var hasCategory = !string.IsNullOrWhiteSpace(category);

            if (!hasSearch && !hasStatus && !hasFamily && !hasCategory)
                return;

            var term = searchTerm?.ToLowerInvariant() ?? string.Empty;
            var parsedStatus = hasStatus ? Enum.Parse<PartStatus>(status!, true) : default;
            var familyLower = productFamily?.ToLowerInvariant() ?? string.Empty;
            var categoryLower = category?.ToLowerInvariant() ?? string.Empty;

            ApplyCriteria(p =>
                (!hasSearch || p.PartNumber.ToLower().Contains(term) || p.Name.ToLower().Contains(term)) &&
                (!hasStatus || p.Status == parsedStatus) &&
                (!hasFamily || (p.ProductFamily != null && p.ProductFamily.ToLower().Contains(familyLower))) &&
                (!hasCategory || (p.Category != null && p.Category.ToLower().Contains(categoryLower))));
        }
    }

    private sealed class PartCountSpecification : Specification<Part>
    {
        public PartCountSpecification(string? searchTerm, string? status, string? productFamily, string? category)
        {
            var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);
            var hasStatus = !string.IsNullOrWhiteSpace(status) && Enum.TryParse<PartStatus>(status, true, out _);
            var hasFamily = !string.IsNullOrWhiteSpace(productFamily);
            var hasCategory = !string.IsNullOrWhiteSpace(category);

            if (!hasSearch && !hasStatus && !hasFamily && !hasCategory)
                return;

            var term = searchTerm?.ToLowerInvariant() ?? string.Empty;
            var parsedStatus = hasStatus ? Enum.Parse<PartStatus>(status!, true) : default;
            var familyLower = productFamily?.ToLowerInvariant() ?? string.Empty;
            var categoryLower = category?.ToLowerInvariant() ?? string.Empty;

            ApplyCriteria(p =>
                (!hasSearch || p.PartNumber.ToLower().Contains(term) || p.Name.ToLower().Contains(term)) &&
                (!hasStatus || p.Status == parsedStatus) &&
                (!hasFamily || (p.ProductFamily != null && p.ProductFamily.ToLower().Contains(familyLower))) &&
                (!hasCategory || (p.Category != null && p.Category.ToLower().Contains(categoryLower))));
        }
    }
}
