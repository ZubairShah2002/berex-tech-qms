using FluentValidation;

namespace BerexQms.Application.AuditManagement.Commands.AddChecklist;

public sealed class AddChecklistCommandValidator : AbstractValidator<AddChecklistCommand>
{
    public AddChecklistCommandValidator()
    {
        RuleFor(x => x.AuditPlanId)
            .NotEmpty().WithMessage("Audit plan ID is required.");

        RuleFor(x => x.AuditRecordId)
            .NotEmpty().WithMessage("Audit record ID is required.");

        RuleFor(x => x.Standard)
            .NotEmpty().WithMessage("Standard is required.")
            .MaximumLength(100).WithMessage("Standard must not exceed 100 characters.");

        RuleFor(x => x.ClauseReference)
            .NotEmpty().WithMessage("Clause reference is required.")
            .MaximumLength(100).WithMessage("Clause reference must not exceed 100 characters.");

        RuleFor(x => x.Requirement)
            .NotEmpty().WithMessage("Requirement is required.")
            .MaximumLength(2000).WithMessage("Requirement must not exceed 2000 characters.");

        RuleFor(x => x.Evidence)
            .MaximumLength(4000).WithMessage("Evidence must not exceed 4000 characters.")
            .When(x => x.Evidence is not null);

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters.")
            .When(x => x.Notes is not null);
    }
}
