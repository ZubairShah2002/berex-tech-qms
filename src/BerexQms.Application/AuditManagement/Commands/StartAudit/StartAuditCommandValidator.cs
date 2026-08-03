using FluentValidation;

namespace BerexQms.Application.AuditManagement.Commands.StartAudit;

public sealed class StartAuditCommandValidator : AbstractValidator<StartAuditCommand>
{
    public StartAuditCommandValidator()
    {
        RuleFor(x => x.AuditPlanId)
            .NotEmpty().WithMessage("Audit plan ID is required.");

        RuleFor(x => x.AuditRecordId)
            .NotEmpty().WithMessage("Audit record ID is required.");
    }
}
