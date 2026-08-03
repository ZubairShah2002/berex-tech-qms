using FluentValidation;

namespace BerexQms.Application.DocumentControl.Commands.AcknowledgeDistribution;

public sealed class AcknowledgeDistributionCommandValidator : AbstractValidator<AcknowledgeDistributionCommand>
{
    public AcknowledgeDistributionCommandValidator()
    {
        RuleFor(x => x.DocumentId).NotEmpty();
        RuleFor(x => x.DistributionId).NotEmpty();
    }
}
