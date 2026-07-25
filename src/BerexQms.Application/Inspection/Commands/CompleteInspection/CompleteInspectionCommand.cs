using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Inspection.Commands.CompleteInspection;

public sealed record CompleteInspectionCommand(Guid InspectionId) : ICommand;
