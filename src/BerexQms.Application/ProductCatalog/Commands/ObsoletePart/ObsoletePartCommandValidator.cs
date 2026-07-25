using FluentValidation;

namespace BerexQms.Application.ProductCatalog.Commands.ObsoletePart;

public sealed class ObsoletePartCommandValidator : AbstractValidator<ObsoletePartCommand>
{
    public ObsoletePartCommandValidator()
    {
        RuleFor(x => x.PartId)
            .NotEmpty().WithMessage("Part ID is required.");
    }
}
