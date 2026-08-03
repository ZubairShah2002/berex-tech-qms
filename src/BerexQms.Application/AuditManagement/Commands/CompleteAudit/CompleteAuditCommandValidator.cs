using FluentValidation;

namespace BerexQms.Application.AuditManagement.Commands.CompleteAudit;

public sealed class CompleteAuditCommandValidator : AbstractValidator<CompleteAuditCommand>
{
    public CompleteAuditCommandValidator()
    {
        RuleFor(x => x.AuditPlanId)
            .NotEmpty().WithMessage("Audit plan ID is required.");

        RuleFor(x => x.AuditRecordId)
            .NotEmpty().WithMessage("Audit record ID is required.");

        RuleFor(x => x.Summary)
            .NotEmpty().WithMessage("Report summary is required.")
            .MaximumLength(4000).WithMessage("Summary must not exceed 4000 characters.");

        RuleFor(x => x.Recommendations)
            .NotEmpty().WithMessage("Recommendations are required.")
            .MaximumLength(4000).WithMessage("Recommendations must not exceed 4000 characters.");

        RuleFor(x => x.AuditorNotes)
            .MaximumLength(4000).WithMessage("Auditor notes must not exceed 4000 characters.")
            .When(x => x.AuditorNotes is not null);
    }
}
