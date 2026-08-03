using FluentValidation;

namespace BerexQms.Application.AuditManagement.Commands.RecordFinding;

public sealed class RecordFindingCommandValidator : AbstractValidator<RecordFindingCommand>
{
    public RecordFindingCommandValidator()
    {
        RuleFor(x => x.AuditPlanId)
            .NotEmpty().WithMessage("Audit plan ID is required.");

        RuleFor(x => x.AuditRecordId)
            .NotEmpty().WithMessage("Audit record ID is required.");

        RuleFor(x => x.Classification)
            .NotEmpty().WithMessage("Finding classification is required.");

        RuleFor(x => x.ClauseReference)
            .NotEmpty().WithMessage("Clause reference is required.")
            .MaximumLength(100).WithMessage("Clause reference must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(4000).WithMessage("Description must not exceed 4000 characters.");

        RuleFor(x => x.Evidence)
            .MaximumLength(4000).WithMessage("Evidence must not exceed 4000 characters.")
            .When(x => x.Evidence is not null);

        RuleFor(x => x.CorrectiveAction)
            .MaximumLength(4000).WithMessage("Corrective action must not exceed 4000 characters.")
            .When(x => x.CorrectiveAction is not null);

        RuleFor(x => x.LinkedCapaId)
            .MaximumLength(200).WithMessage("Linked CAPA ID must not exceed 200 characters.")
            .When(x => x.LinkedCapaId is not null);
    }
}
