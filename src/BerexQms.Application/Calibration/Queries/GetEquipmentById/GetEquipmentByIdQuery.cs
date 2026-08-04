using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Calibration.DTOs;

namespace BerexQms.Application.Calibration.Queries.GetEquipmentById;

public sealed record GetEquipmentByIdQuery(Guid EquipmentId) : IQuery<EquipmentDetailDto>;
