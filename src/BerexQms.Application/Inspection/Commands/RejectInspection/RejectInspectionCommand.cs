using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Inspection.Commands.RejectInspection;

public sealed record RejectInspectionCommand(Guid InspectionId, string? Notes) : ICommand;
