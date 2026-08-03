using FluentValidation;

namespace BerexQms.Application.DocumentControl.Commands.ReleaseVersion;

public sealed class ReleaseVersionCommandValidator : AbstractValidator<ReleaseVersionCommand>
{
    public ReleaseVersionCommandValidator()
    {
        RuleFor(x => x.DocumentId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
        RuleFor(x => x.EffectiveDate).NotEmpty();
    }
}
