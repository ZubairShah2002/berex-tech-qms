using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Calibration.DTOs;
using BerexQms.Domain.Calibration.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Calibration.Queries.GetOverdueEquipment;

internal sealed class GetOverdueEquipmentQueryHandler
    : IQueryHandler<GetOverdueEquipmentQuery, IReadOnlyList<EquipmentDto>>
{
    private readonly IEquipmentRepository _repository;

    public GetOverdueEquipmentQueryHandler(IEquipmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<EquipmentDto>>> Handle(
        GetOverdueEquipmentQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.GetOverdueEquipmentAsync(cancellationToken);

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

        return dtos;
    }
}
