using FluentValidation;

namespace BerexQms.Application.Training.Commands.ManageCompetency;

public sealed class ManageCompetencyCommandValidator : AbstractValidator<ManageCompetencyCommand>
{
    private static readonly string[] ValidActions =
        ["START_TRAINING", "QUALIFY", "SUSPEND", "REVOKE", "EXPIRE"];

    public ManageCompetencyCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.QualificationId).NotEmpty();
        RuleFor(x => x.Action).NotEmpty()
            .Must(a => ValidActions.Contains(a.ToUpperInvariant()))
            .WithMessage("Action must be one of: START_TRAINING, QUALIFY, SUSPEND, REVOKE, EXPIRE.");
    }
}
