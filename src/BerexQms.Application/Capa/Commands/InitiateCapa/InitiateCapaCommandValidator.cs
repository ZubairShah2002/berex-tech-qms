using FluentValidation;

namespace BerexQms.Application.Capa.Commands.InitiateCapa;

public sealed class InitiateCapaCommandValidator : AbstractValidator<InitiateCapaCommand>
{
    public InitiateCapaCommandValidator()
    {
        RuleFor(x => x.CapaNumber)
            .NotEmpty().WithMessage("CAPA number is required.")
            .MaximumLength(50).WithMessage("CAPA number must not exceed 50 characters.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("CAPA title is required.")
            .MaximumLength(200).WithMessage("CAPA title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("CAPA description is required.")
            .MaximumLength(4000).WithMessage("Description must not exceed 4000 characters.");

        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Priority is required.");

        RuleFor(x => x.SourceType)
            .NotEmpty().WithMessage("Source type is required.");
    }
}
