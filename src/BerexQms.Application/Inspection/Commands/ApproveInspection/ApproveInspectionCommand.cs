using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Inspection.Commands.ApproveInspection;

public sealed record ApproveInspectionCommand(Guid InspectionId) : ICommand;
