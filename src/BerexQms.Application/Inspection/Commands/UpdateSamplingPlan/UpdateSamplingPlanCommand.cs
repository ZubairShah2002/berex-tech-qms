using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Inspection.Commands.UpdateSamplingPlan;

public sealed record UpdateSamplingPlanCommand(
    Guid SamplingPlanId,
    string Level,
    decimal AqlValue,
    int SampleSize,
    int AcceptNumber,
    int RejectNumber) : ICommand;
