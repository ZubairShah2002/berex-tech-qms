using FluentValidation;

namespace BerexQms.Application.Training.Commands.UpdateQualification;

public sealed class UpdateQualificationCommandValidator : AbstractValidator<UpdateQualificationCommand>
{
    public UpdateQualificationCommandValidator()
    {
        RuleFor(x => x.QualificationId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ValidityMonths).GreaterThan(0);
        RuleFor(x => x.RenewalWindowDays).GreaterThanOrEqualTo(0);
    }
}
