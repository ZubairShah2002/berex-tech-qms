using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.DocumentControl.DTOs;
using BerexQms.Domain.DocumentControl.Entities;
using BerexQms.Domain.DocumentControl.Enums;
using BerexQms.Domain.DocumentControl.Repositories;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.DocumentControl.Queries.ListDocuments;

internal sealed class ListDocumentsQueryHandler
    : IQueryHandler<ListDocumentsQuery, PagedResult<DocumentDto>>
{
    private readonly IDocumentRepository _repository;

    public ListDocumentsQueryHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<DocumentDto>>> Handle(
        ListDocumentsQuery request, CancellationToken cancellationToken)
    {
        var spec = new DocumentListSpecification(
            request.SearchTerm, request.DocumentType, request.Status, request.IsActive,
            request.Page, request.PageSize);

        var records = await _repository.ListAsync(spec, cancellationToken);

        var countSpec = new DocumentCountSpecification(
            request.SearchTerm, request.DocumentType, request.Status, request.IsActive);
        var totalCount = await _repository.CountAsync(countSpec, cancellationToken);

        var dtos = records.Select(r =>
        {
            var releasedVersion = r.Versions
                .FirstOrDefault(v => v.Status == DocumentStatus.Released);
            var latestVersion = releasedVersion ?? r.Versions
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefault();

            return new DocumentDto(
                r.Id,
                r.DocumentNumber,
                r.Title,
                r.DocumentType.ToString(),
                r.OwnerId,
                r.Department,
                r.IsActive,
                r.Versions.Count,
                latestVersion?.VersionNumber,
                latestVersion?.Status.ToString(),
                r.CreatedAt);
        }).ToList();

        return new PagedResult<DocumentDto>(dtos, totalCount, request.Page, request.PageSize);
    }

    private sealed class DocumentListSpecification : Specification<DocumentMaster>
    {
        public DocumentListSpecification(
            string? searchTerm, string? documentType, string? status, bool? isActive,
            int page, int pageSize)
        {
            ApplyFilters(searchTerm, documentType, status, isActive);
            AddInclude(d => d.Versions);
            ApplyOrderByDescending(d => d.CreatedAt);
            ApplyPaging((page - 1) * pageSize, pageSize);
        }

        private void ApplyFilters(string? searchTerm, string? documentType, string? status, bool? isActive)
        {
            var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);
            var hasType = !string.IsNullOrWhiteSpace(documentType) &&
                          Enum.TryParse<DocumentType>(documentType, true, out _);
            var hasStatus = !string.IsNullOrWhiteSpace(status) &&
                            Enum.TryParse<DocumentStatus>(status, true, out _);
            var hasActive = isActive.HasValue;

            if (!hasSearch && !hasType && !hasStatus && !hasActive)
                return;

            var term = searchTerm?.ToUpperInvariant() ?? string.Empty;
            var parsedType = hasType ? Enum.Parse<DocumentType>(documentType!, true) : default;
            var parsedStatus = hasStatus ? Enum.Parse<DocumentStatus>(status!, true) : default;

            ApplyCriteria(d =>
                (!hasSearch || d.DocumentNumber.ToUpper().Contains(term) || d.Title.ToUpper().Contains(term)) &&
                (!hasType || d.DocumentType == parsedType) &&
                (!hasStatus || d.Versions.Any(v => v.Status == parsedStatus)) &&
                (!hasActive || d.IsActive == isActive!.Value));
        }
    }

    private sealed class DocumentCountSpecification : Specification<DocumentMaster>
    {
        public DocumentCountSpecification(string? searchTerm, string? documentType, string? status, bool? isActive)
        {
            var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);
            var hasType = !string.IsNullOrWhiteSpace(documentType) &&
                          Enum.TryParse<DocumentType>(documentType, true, out _);
            var hasStatus = !string.IsNullOrWhiteSpace(status) &&
                            Enum.TryParse<DocumentStatus>(status, true, out _);
            var hasActive = isActive.HasValue;

            if (!hasSearch && !hasType && !hasStatus && !hasActive)
                return;

            var term = searchTerm?.ToUpperInvariant() ?? string.Empty;
            var parsedType = hasType ? Enum.Parse<DocumentType>(documentType!, true) : default;
            var parsedStatus = hasStatus ? Enum.Parse<DocumentStatus>(status!, true) : default;

            ApplyCriteria(d =>
                (!hasSearch || d.DocumentNumber.ToUpper().Contains(term) || d.Title.ToUpper().Contains(term)) &&
                (!hasType || d.DocumentType == parsedType) &&
                (!hasStatus || d.Versions.Any(v => v.Status == parsedStatus)) &&
                (!hasActive || d.IsActive == isActive!.Value));
        }
    }
}
