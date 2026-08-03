using FluentValidation;

namespace BerexQms.Application.AuditManagement.Commands.AddAudit;

public sealed class AddAuditCommandValidator : AbstractValidator<AddAuditCommand>
{
    public AddAuditCommandValidator()
    {
        RuleFor(x => x.AuditPlanId)
            .NotEmpty().WithMessage("Audit plan ID is required.");

        RuleFor(x => x.AuditNumber)
            .NotEmpty().WithMessage("Audit number is required.")
            .MaximumLength(50).WithMessage("Audit number must not exceed 50 characters.");

        RuleFor(x => x.AuditType)
            .NotEmpty().WithMessage("Audit type is required.");

        RuleFor(x => x.LeadAuditorId)
            .NotEmpty().WithMessage("Lead auditor is required.");

        RuleFor(x => x.AuditeeArea)
            .MaximumLength(200).WithMessage("Auditee area must not exceed 200 characters.")
            .When(x => x.AuditeeArea is not null);
    }
}
