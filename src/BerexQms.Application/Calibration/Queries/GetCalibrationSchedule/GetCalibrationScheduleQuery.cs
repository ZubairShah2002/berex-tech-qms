using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Calibration.DTOs;

namespace BerexQms.Application.Calibration.Queries.GetCalibrationSchedule;

public sealed record GetCalibrationScheduleQuery() : IQuery<IReadOnlyList<EquipmentDto>>;
