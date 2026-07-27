using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.NonConformance.Commands.RecordDisposition;

public sealed record RecordDispositionCommand(
    Guid NonConformanceId,
    string Type,
    string Justification) : ICommand;
