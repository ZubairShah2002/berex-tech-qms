using FluentValidation;

namespace BerexQms.Application.Capa.Commands.AddAction;

public sealed class AddActionCommandValidator : AbstractValidator<AddActionCommand>
{
    public AddActionCommandValidator()
    {
        RuleFor(x => x.CapaId)
            .NotEmpty().WithMessage("CAPA ID is required.");

        RuleFor(x => x.ActionType)
            .NotEmpty().WithMessage("Action type is required.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Action description is required.")
            .MaximumLength(4000).WithMessage("Action description must not exceed 4000 characters.");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("Action owner is required.");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future.");

        RuleFor(x => x.EvidenceRequirement)
            .MaximumLength(2000).WithMessage("Evidence requirement must not exceed 2000 characters.")
            .When(x => x.EvidenceRequirement is not null);
    }
}
