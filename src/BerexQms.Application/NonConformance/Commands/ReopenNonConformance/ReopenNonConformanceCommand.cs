using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.NonConformance.Commands.ReopenNonConformance;

public sealed record ReopenNonConformanceCommand(
    Guid NonConformanceId,
    string Reason) : ICommand;
