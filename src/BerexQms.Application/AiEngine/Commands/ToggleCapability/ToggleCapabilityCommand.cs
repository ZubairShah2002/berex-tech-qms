using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.AiEngine.Commands.ToggleCapability;

public sealed record ToggleCapabilityCommand(string Capability, bool Enable) : ICommand;
