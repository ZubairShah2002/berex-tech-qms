using FluentValidation;

namespace BerexQms.Application.DocumentControl.Commands.AddDistribution;

public sealed class AddDistributionCommandValidator : AbstractValidator<AddDistributionCommand>
{
    public AddDistributionCommandValidator()
    {
        RuleFor(x => x.DocumentId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
        RuleFor(x => x.RecipientId).NotEmpty();
        RuleFor(x => x.ComplianceDeadline).NotEmpty();
    }
}
