using FluentValidation;

namespace BerexQms.Application.Identity.Commands.UpdateUser;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.")
            .When(x => x.PhoneNumber is not null);

        RuleFor(x => x.Department)
            .MaximumLength(100).WithMessage("Department cannot exceed 100 characters.")
            .When(x => x.Department is not null);

        RuleFor(x => x.JobTitle)
            .MaximumLength(100).WithMessage("Job title cannot exceed 100 characters.")
            .When(x => x.JobTitle is not null);
    }
}
