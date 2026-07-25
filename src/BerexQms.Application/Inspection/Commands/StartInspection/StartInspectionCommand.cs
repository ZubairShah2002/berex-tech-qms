using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Inspection.Commands.StartInspection;

public sealed record StartInspectionCommand(Guid InspectionId) : ICommand;
