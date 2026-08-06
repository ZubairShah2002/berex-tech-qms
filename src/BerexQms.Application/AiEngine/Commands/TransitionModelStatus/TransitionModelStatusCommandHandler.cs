using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Commands.TransitionModelStatus;

/// <summary>
/// Applies a validated status transition to an AI model's lifecycle state machine:
/// Training -> Validating -> Shadow -> Active -> Deprecated -> Retired, with a
/// "-> Retired" escape hatch valid from Active, Shadow, or Deprecated (mirroring the
/// transitions <see cref="Domain.AiEngine.Entities.AiModel"/> itself enforces). Promoting
/// a model to Active automatically deprecates whichever model was previously active for
/// the same capability, since only one active model may serve a capability at a time.
/// </summary>
internal sealed class TransitionModelStatusCommandHandler : ICommandHandler<TransitionModelStatusCommand>
{
    private readonly IAiModelRepository _repository;

    public TransitionModelStatusCommandHandler(IAiModelRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(TransitionModelStatusCommand request, CancellationToken cancellationToken)
    {
        var model = await _repository.GetByIdAsync(request.ModelId, cancellationToken);
        if (model is null)
            return Result.Failure(AiEngineErrors.ModelNotFound);

        if (!Enum.TryParse<ModelStatus>(request.TargetStatus, true, out var targetStatus))
            return Result.Failure(AiEngineErrors.InvalidModelStatus);

        if (!Enum.TryParse<ModelStatus>(model.Status, true, out var currentStatus))
            return Result.Failure(AiEngineErrors.InvalidModelStatus);

        switch (currentStatus, targetStatus)
        {
            case (ModelStatus.Training, ModelStatus.Validating):
                model.StartValidation();
                break;

            case (ModelStatus.Validating, ModelStatus.Shadow):
                model.PromoteToShadow();
                break;

            case (ModelStatus.Shadow, ModelStatus.Active):
                var currentActive = await _repository.GetActiveModelAsync(model.Capability, cancellationToken);
                if (currentActive is not null && currentActive.Id != model.Id)
                {
                    currentActive.Deprecate();
                    await _repository.UpdateAsync(currentActive, cancellationToken);
                }

                model.Activate();
                break;

            case (ModelStatus.Active, ModelStatus.Deprecated):
                model.Deprecate();
                break;

            case (ModelStatus.Active, ModelStatus.Retired):
            case (ModelStatus.Shadow, ModelStatus.Retired):
            case (ModelStatus.Deprecated, ModelStatus.Retired):
                model.Retire();
                break;

            default:
                return Result.Failure(AiEngineErrors.InvalidModelTransition);
        }

        await _repository.UpdateAsync(model, cancellationToken);

        return Result.Success();
    }
}
