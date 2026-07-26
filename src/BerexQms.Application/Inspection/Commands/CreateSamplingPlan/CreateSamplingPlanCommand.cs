using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Inspection.DTOs;

namespace BerexQms.Application.Inspection.Commands.CreateSamplingPlan;

public sealed record CreateSamplingPlanCommand(
    Guid PartId,
    Guid? SupplierId,
    string InspectionType,
    string Level,
    decimal AqlValue,
    int SampleSize,
    int AcceptNumber,
    int RejectNumber) : ICommand<SamplingPlanDto>;
