using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Commands.RecordUserAction;

internal sealed class RecordUserActionCommandHandler : ICommandHandler<RecordUserActionCommand>
{
    private readonly IAiInteractionRepository _repository;

    public RecordUserActionCommandHandler(IAiInteractionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(RecordUserActionCommand request, CancellationToken cancellationToken)
    {
        var interaction = await _repository.GetByIdAsync(request.InteractionId, cancellationToken);
        if (interaction is null)
            return Result.Failure(AiEngineErrors.InteractionNotFound);

        if (!Enum.TryParse<AiUserAction>(request.Action, true, out var action))
            return Result.Failure(AiEngineErrors.InvalidUserAction);

        if (interaction.Status != AiInteractionStatus.Completed.ToString())
            return Result.Failure(AiEngineErrors.InteractionNotCompleted);

        // Accepting a moderate-confidence suggestion requires an explicit justification
        // from the user before it can be acted on. Mirrors the invariant enforced inside
        // AiInteraction.RecordUserAction so callers get a Result failure instead of a
        // thrown DomainException.
        var isModerateConfidence = interaction.Confidence?.Level == ConfidenceLevel.Moderate;

        if (action == AiUserAction.Accepted && isModerateConfidence &&
            string.IsNullOrWhiteSpace(request.Justification))
        {
            return Result.Failure(AiEngineErrors.JustificationRequired);
        }

        interaction.RecordUserAction(action, request.Justification);

        await _repository.UpdateAsync(interaction, cancellationToken);

        return Result.Success();
    }
}
