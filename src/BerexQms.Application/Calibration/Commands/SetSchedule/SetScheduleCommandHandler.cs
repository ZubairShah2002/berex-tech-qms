using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Calibration.DTOs;
using BerexQms.Domain.Calibration.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Calibration.Commands.SetSchedule;

internal sealed class SetScheduleCommandHandler
    : ICommandHandler<SetScheduleCommand, CalibrationScheduleDto>
{
    private readonly IEquipmentRepository _repository;

    public SetScheduleCommandHandler(IEquipmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CalibrationScheduleDto>> Handle(
        SetScheduleCommand request, CancellationToken cancellationToken)
    {
        var equipment = await _repository.GetWithScheduleAsync(request.EquipmentId, cancellationToken);
        if (equipment is null)
            return CalibrationErrors.EquipmentNotFound;

        var schedule = equipment.SetSchedule(
            request.IntervalDays,
            request.LeadTimeDays,
            request.LabType,
            request.ProcedureRef,
            request.NextDueDate);

        return new CalibrationScheduleDto(
            schedule.Id,
            schedule.IntervalDays,
            schedule.LeadTimeDays,
            schedule.LabType,
            schedule.ProcedureRef,
            schedule.NextDueDate);
    }
}
