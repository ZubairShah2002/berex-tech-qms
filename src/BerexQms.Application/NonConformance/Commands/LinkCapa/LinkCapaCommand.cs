using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.NonConformance.Commands.LinkCapa;

public sealed record LinkCapaCommand(
    Guid NonConformanceId,
    Guid CapaId) : ICommand;
