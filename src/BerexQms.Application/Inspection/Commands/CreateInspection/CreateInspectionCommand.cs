using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Inspection.DTOs;

namespace BerexQms.Application.Inspection.Commands.CreateInspection;

public sealed record CreateInspectionCommand(
    string InspectionNumber,
    string Type,
    Guid PartId,
    Guid? PartRevisionId,
    string? LotNumber,
    int? LotSize,
    int? SampleSize,
    Guid? SupplierId,
    Guid? SamplingPlanId,
    string InspectorId) : ICommand<InspectionDto>;
