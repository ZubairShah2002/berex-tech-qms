using FluentValidation;

namespace BerexQms.Application.DocumentControl.Commands.MakeObsolete;

public sealed class MakeObsoleteCommandValidator : AbstractValidator<MakeObsoleteCommand>
{
    public MakeObsoleteCommandValidator()
    {
        RuleFor(x => x.DocumentId).NotEmpty();
    }
}
