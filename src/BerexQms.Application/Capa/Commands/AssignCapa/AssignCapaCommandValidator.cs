using FluentValidation;

namespace BerexQms.Application.Capa.Commands.AssignCapa;

public sealed class AssignCapaCommandValidator : AbstractValidator<AssignCapaCommand>
{
    public AssignCapaCommandValidator()
    {
        RuleFor(x => x.CapaId)
            .NotEmpty().WithMessage("CAPA ID is required.");

        RuleFor(x => x.AssigneeId)
            .NotEmpty().WithMessage("Assignee ID is required.");
    }
}
