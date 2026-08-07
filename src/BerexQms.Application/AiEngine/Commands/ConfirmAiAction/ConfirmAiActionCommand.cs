using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Commands.ConfirmAiAction;

public sealed record ConfirmAiActionCommand(
    Guid ActionLogId,
    bool Confirm) : ICommand<AiActionLogDto>;
