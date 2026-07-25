using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Inspection.Commands.ToggleSamplingPlan;

public sealed record ToggleSamplingPlanCommand(Guid SamplingPlanId, bool Activate) : ICommand;
