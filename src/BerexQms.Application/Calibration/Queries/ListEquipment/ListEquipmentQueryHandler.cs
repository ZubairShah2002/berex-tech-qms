using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Calibration.DTOs;
using BerexQms.Domain.Calibration.Entities;
using BerexQms.Domain.Calibration.Repositories;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Calibration.Queries.ListEquipment;

internal sealed class ListEquipmentQueryHandler
    : IQueryHandler<ListEquipmentQuery, PagedResult<EquipmentDto>>
{
    private readonly IEquipmentRepository _repository;

    public ListEquipmentQueryHandler(IEquipmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<EquipmentDto>>> Handle(
        ListEquipmentQuery request, CancellationToken cancellationToken)
    {
        var spec = new EquipmentListSpecification(
            request.SearchTerm, request.Status, request.Page, request.PageSize);

        var items = await _repository.ListAsync(spec, cancellationToken);

        var countSpec = new EquipmentCountSpecification(request.SearchTerm, request.Status);
        var totalCount = await _repository.CountAsync(countSpec, cancellationToken);

        var dtos = items.Select(e => new EquipmentDto(
            e.Id,
            e.Code,
            e.Name,
            e.Type,
            e.Manufacturer,
            e.Model,
            e.SerialNumber,
            e.Status,
            e.Location,
            e.Assignment?.Department,
            e.Schedule?.NextDueDate,
            e.Calibrations.Count,
            e.CreatedAt)).ToList();

        return new PagedResult<EquipmentDto>(dtos, totalCount, request.Page, request.PageSize);
    }

    private sealed class EquipmentListSpecification : Specification<Equipment>
    {
        public EquipmentListSpecification(string? searchTerm, string? status, int page, int pageSize)
        {
            ApplyFilters(searchTerm, status);
            ApplyOrderByDescending(e => e.CreatedAt);
            ApplyPaging((page - 1) * pageSize, pageSize);
            AddInclude(e => e.Calibrations);
        }

        private void ApplyFilters(string? searchTerm, string? status)
        {
            var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);
            var hasStatus = !string.IsNullOrWhiteSpace(status);

            if (!hasSearch && !hasStatus)
                return;

            var term = searchTerm?.ToUpperInvariant() ?? string.Empty;

            ApplyCriteria(e =>
                (!hasSearch || e.Code.ToUpper().Contains(term) || e.Name.ToUpper().Contains(term)) &&
                (!hasStatus || e.Status == status));
        }
    }

    private sealed class EquipmentCountSpecification : Specification<Equipment>
    {
        public EquipmentCountSpecification(string? searchTerm, string? status)
        {
            var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);
            var hasStatus = !string.IsNullOrWhiteSpace(status);

            if (!hasSearch && !hasStatus)
                return;

            var term = searchTerm?.ToUpperInvariant() ?? string.Empty;

            ApplyCriteria(e =>
                (!hasSearch || e.Code.ToUpper().Contains(term) || e.Name.ToUpper().Contains(term)) &&
                (!hasStatus || e.Status == status));
        }
    }
}
