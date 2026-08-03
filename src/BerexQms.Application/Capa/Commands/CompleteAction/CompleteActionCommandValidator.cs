using FluentValidation;

namespace BerexQms.Application.Capa.Commands.CompleteAction;

public sealed class CompleteActionCommandValidator : AbstractValidator<CompleteActionCommand>
{
    public CompleteActionCommandValidator()
    {
        RuleFor(x => x.CapaId)
            .NotEmpty().WithMessage("CAPA ID is required.");

        RuleFor(x => x.ActionId)
            .NotEmpty().WithMessage("Action ID is required.");

        RuleFor(x => x.CompletionNotes)
            .MaximumLength(4000).WithMessage("Completion notes must not exceed 4000 characters.")
            .When(x => x.CompletionNotes is not null);

        RuleFor(x => x.EvidenceProvided)
            .MaximumLength(4000).WithMessage("Evidence provided must not exceed 4000 characters.")
            .When(x => x.EvidenceProvided is not null);
    }
}
