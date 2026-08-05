using FluentValidation;

namespace BerexQms.Application.Training.Commands.CreateAssignment;

public sealed class CreateAssignmentCommandValidator : AbstractValidator<CreateAssignmentCommand>
{
    public CreateAssignmentCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.DueDate).GreaterThan(DateTime.UtcNow)
            .WithMessage("Due date must be in the future.");
    }
}
