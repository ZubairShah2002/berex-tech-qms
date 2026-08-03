using FluentValidation;

namespace BerexQms.Application.Capa.Commands.ScheduleVerification;

public sealed class ScheduleVerificationCommandValidator : AbstractValidator<ScheduleVerificationCommand>
{
    public ScheduleVerificationCommandValidator()
    {
        RuleFor(x => x.CapaId)
            .NotEmpty().WithMessage("CAPA ID is required.");

        RuleFor(x => x.ScheduledDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Scheduled date must be in the future.");

        RuleFor(x => x.VerificationCriteria)
            .NotEmpty().WithMessage("Verification criteria are required.")
            .MaximumLength(4000).WithMessage("Verification criteria must not exceed 4000 characters.");
    }
}
