using FluentValidation;

namespace BerexQms.Application.NonConformance.Commands.LinkCapa;

public sealed class LinkCapaCommandValidator : AbstractValidator<LinkCapaCommand>
{
    public LinkCapaCommandValidator()
    {
        RuleFor(x => x.NonConformanceId)
            .NotEmpty().WithMessage("Non-conformance ID is required.");

        RuleFor(x => x.CapaId)
            .NotEmpty().WithMessage("CAPA ID is required.");
    }
}
