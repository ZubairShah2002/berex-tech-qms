using BerexQms.Domain.AiEngine.Enums;
using FluentValidation;

namespace BerexQms.Application.AiEngine.Commands.RecordUserAction;

public sealed class RecordUserActionCommandValidator : AbstractValidator<RecordUserActionCommand>
{
    public RecordUserActionCommandValidator()
    {
        RuleFor(x => x.InteractionId)
            .NotEmpty().WithMessage("Interaction ID is required.");

        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("Action is required.")
            .Must(v => Enum.TryParse<AiUserAction>(v, true, out _))
            .WithMessage("Invalid user action. Valid values: Accepted, Rejected, Modified, " +
                         "Ignored, Suppressed.");
    }
}
