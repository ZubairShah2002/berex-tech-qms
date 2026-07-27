using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.NonConformance.Commands.VerifyContainment;

public sealed record VerifyContainmentCommand(
    Guid NonConformanceId,
    Guid ContainmentActionId) : ICommand;
