using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Calibration.DTOs;

namespace BerexQms.Application.Calibration.Queries.GetOverdueEquipment;

public sealed record GetOverdueEquipmentQuery() : IQuery<IReadOnlyList<EquipmentDto>>;
