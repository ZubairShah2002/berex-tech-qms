using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Inspection.Commands.CancelInspection;

public sealed record CancelInspectionCommand(Guid InspectionId) : ICommand;
