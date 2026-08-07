using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.AiEngine.Commands.TransitionModelStatus;

public sealed record TransitionModelStatusCommand(Guid ModelId, string TargetStatus) : ICommand;
