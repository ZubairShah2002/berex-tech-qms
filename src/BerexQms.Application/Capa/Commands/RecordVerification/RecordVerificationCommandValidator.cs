using FluentValidation;

namespace BerexQms.Application.Capa.Commands.RecordVerification;

public sealed class RecordVerificationCommandValidator : AbstractValidator<RecordVerificationCommand>
{
    public RecordVerificationCommandValidator()
    {
        RuleFor(x => x.CapaId)
            .NotEmpty().WithMessage("CAPA ID is required.");

        RuleFor(x => x.VerificationId)
            .NotEmpty().WithMessage("Verification ID is required.");

        RuleFor(x => x.Result)
            .NotEmpty().WithMessage("Verification result is required.")
            .MaximumLength(4000).WithMessage("Verification result must not exceed 4000 characters.");

        RuleFor(x => x.Evidence)
            .MaximumLength(4000).WithMessage("Evidence must not exceed 4000 characters.")
            .When(x => x.Evidence is not null);
    }
}
