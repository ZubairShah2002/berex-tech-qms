using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Inspection.Commands.SetDisposition;

public sealed record SetDispositionCommand(
    Guid InspectionId,
    string Type,
    string Justification) : ICommand;
