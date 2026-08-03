using FluentValidation;

namespace BerexQms.Application.AuditManagement.Commands.CancelAudit;

public sealed class CancelAuditCommandValidator : AbstractValidator<CancelAuditCommand>
{
    public CancelAuditCommandValidator()
    {
        RuleFor(x => x.AuditPlanId)
            .NotEmpty().WithMessage("Audit plan ID is required.");

        RuleFor(x => x.AuditRecordId)
            .NotEmpty().WithMessage("Audit record ID is required.");
    }
}
