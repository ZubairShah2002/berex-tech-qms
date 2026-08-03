using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Capa.DTOs;
using BerexQms.Domain.Capa.Enums;
using BerexQms.Domain.Capa.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Capa.Commands.AddAction;

public sealed class AddActionCommandHandler : ICommandHandler<AddActionCommand, CapaActionDto>
{
    private readonly ICAPARepository _repository;

    public AddActionCommandHandler(ICAPARepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CapaActionDto>> Handle(AddActionCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetWithActionsAsync(request.CapaId, cancellationToken);
        if (record is null)
            return CAPAErrors.NotFound;

        if (!Enum.TryParse<ActionType>(request.ActionType, true, out var actionType))
            return Error.Validation("CAPA.InvalidActionType", $"Invalid action type: {request.ActionType}.");

        var action = record.AddAction(
            actionType, request.Description, request.OwnerId,
            request.DueDate, request.EvidenceRequirement);

        await _repository.UpdateAsync(record, cancellationToken);

        return new CapaActionDto(
            action.Id,
            action.ActionType.ToString(),
            action.Description,
            action.OwnerId,
            action.DueDate,
            action.EvidenceRequirement,
            action.CompletionNotes,
            action.EvidenceProvided,
            action.CompletedAt,
            action.CompletedBy,
            action.IsOverdue,
            action.CreatedAt);
    }
}
