using FluentValidation;

namespace BerexQms.Application.Training.Commands.CompleteAssignment;

public sealed class CompleteAssignmentCommandValidator : AbstractValidator<CompleteAssignmentCommand>
{
    public CompleteAssignmentCommandValidator()
    {
        RuleFor(x => x.AssignmentId).NotEmpty();
        RuleFor(x => x.CompletionDate).NotEmpty();
        RuleFor(x => x.Result).NotEmpty()
            .Must(r => r == "Pass" || r == "Fail")
            .WithMessage("Result must be 'Pass' or 'Fail'.");
    }
}
