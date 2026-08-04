using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Calibration.DTOs;
using BerexQms.Domain.Calibration.Entities;
using BerexQms.Domain.Calibration.Repositories;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Calibration.Queries.GetCalibrationSchedule;

internal sealed class GetCalibrationScheduleQueryHandler
    : IQueryHandler<GetCalibrationScheduleQuery, IReadOnlyList<EquipmentDto>>
{
    private readonly IEquipmentRepository _repository;

    public GetCalibrationScheduleQueryHandler(IEquipmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<EquipmentDto>>> Handle(
        GetCalibrationScheduleQuery request, CancellationToken cancellationToken)
    {
        var spec = new ScheduledEquipmentSpecification();
        var items = await _repository.ListAsync(spec, cancellationToken);

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

    private sealed class ScheduledEquipmentSpecification : Specification<Equipment>
    {
        public ScheduledEquipmentSpecification()
        {
            ApplyCriteria(e =>
                e.Status != "Retired" &&
                e.Schedule != null);
            ApplyOrderBy(e => e.Schedule!.NextDueDate);
            AddInclude(e => e.Calibrations);
        }
    }
}
