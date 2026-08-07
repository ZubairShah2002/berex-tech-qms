using FluentValidation;

namespace BerexQms.Application.AiEngine.Commands.AssignAiPermissionLevel;

public sealed class AssignAiPermissionLevelCommandValidator
    : AbstractValidator<AssignAiPermissionLevelCommand>
{
    public AssignAiPermissionLevelCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.PermissionLevel).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
