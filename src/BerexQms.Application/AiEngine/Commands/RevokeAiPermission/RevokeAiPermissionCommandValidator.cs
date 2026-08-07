using FluentValidation;

namespace BerexQms.Application.AiEngine.Commands.RevokeAiPermission;

public sealed class RevokeAiPermissionCommandValidator
    : AbstractValidator<RevokeAiPermissionCommand>
{
    public RevokeAiPermissionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
