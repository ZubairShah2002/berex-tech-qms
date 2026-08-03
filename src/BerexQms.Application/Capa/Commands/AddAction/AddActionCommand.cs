using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Capa.DTOs;

namespace BerexQms.Application.Capa.Commands.AddAction;

public sealed record AddActionCommand(
    Guid CapaId,
    string ActionType,
    string Description,
    string OwnerId,
    DateTime DueDate,
    string? EvidenceRequirement) : ICommand<CapaActionDto>;
