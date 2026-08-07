using BerexQms.Domain.AiEngine.Enums;
using FluentValidation;

namespace BerexQms.Application.AiEngine.Commands.TransitionModelStatus;

public sealed class TransitionModelStatusCommandValidator : AbstractValidator<TransitionModelStatusCommand>
{
    public TransitionModelStatusCommandValidator()
    {
        RuleFor(x => x.ModelId)
            .NotEmpty().WithMessage("Model ID is required.");

        RuleFor(x => x.TargetStatus)
            .NotEmpty().WithMessage("Target status is required.")
            .Must(v => Enum.TryParse<ModelStatus>(v, true, out _))
            .WithMessage("Invalid target status. Valid values: Training, Validating, Shadow, " +
                         "Active, Deprecated, Retired.");
    }
}
