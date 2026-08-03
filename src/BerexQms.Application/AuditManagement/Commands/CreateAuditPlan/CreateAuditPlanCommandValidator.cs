using FluentValidation;

namespace BerexQms.Application.AuditManagement.Commands.CreateAuditPlan;

public sealed class CreateAuditPlanCommandValidator : AbstractValidator<CreateAuditPlanCommand>
{
    public CreateAuditPlanCommandValidator()
    {
        RuleFor(x => x.PlanName)
            .NotEmpty().WithMessage("Plan name is required.")
            .MaximumLength(200).WithMessage("Plan name must not exceed 200 characters.");

        RuleFor(x => x.Year)
            .GreaterThan(2020).WithMessage("Plan year must be greater than 2020.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.Scope)
            .MaximumLength(2000).WithMessage("Scope must not exceed 2000 characters.")
            .When(x => x.Scope is not null);
    }
}
