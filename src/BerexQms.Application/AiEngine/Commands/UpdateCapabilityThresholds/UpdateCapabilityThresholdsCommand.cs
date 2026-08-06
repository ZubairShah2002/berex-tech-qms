using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.AiEngine.Commands.UpdateCapabilityThresholds;

public sealed record UpdateCapabilityThresholdsCommand(
    string Capability,
    decimal LowThreshold,
    decimal ModerateThreshold,
    decimal HighThreshold) : ICommand;
