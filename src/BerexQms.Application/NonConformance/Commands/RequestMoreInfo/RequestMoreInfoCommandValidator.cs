using FluentValidation;

namespace BerexQms.Application.NonConformance.Commands.RequestMoreInfo;

public sealed class RequestMoreInfoCommandValidator : AbstractValidator<RequestMoreInfoCommand>
{
    public RequestMoreInfoCommandValidator()
    {
        RuleFor(x => x.NonConformanceId)
            .NotEmpty().WithMessage("Non-conformance ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(4000).WithMessage("Reason cannot exceed 4000 characters.");
    }
}
