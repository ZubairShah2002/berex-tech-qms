using FluentValidation;

namespace BerexQms.Application.Identity.Commands.CreateTenant;

public sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tenant name is required.")
            .MaximumLength(200).WithMessage("Tenant name cannot exceed 200 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Tenant code is required.")
            .MaximumLength(20).WithMessage("Tenant code cannot exceed 20 characters.")
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Tenant code must contain only alphanumeric characters, hyphens, or underscores.");

        RuleFor(x => x.ContactEmail)
            .EmailAddress().WithMessage("A valid email is required.")
            .MaximumLength(254).WithMessage("Email cannot exceed 254 characters.")
            .When(x => x.ContactEmail is not null);

        RuleFor(x => x.TimeZone)
            .MaximumLength(50).WithMessage("Timezone cannot exceed 50 characters.")
            .When(x => x.TimeZone is not null);
    }
}
