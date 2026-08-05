using FluentValidation;

namespace BerexQms.Application.Training.Commands.CreateQualification;

public sealed class CreateQualificationCommandValidator : AbstractValidator<CreateQualificationCommand>
{
    public CreateQualificationCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ValidityMonths).GreaterThan(0);
        RuleFor(x => x.RenewalWindowDays).GreaterThanOrEqualTo(0);
    }
}
