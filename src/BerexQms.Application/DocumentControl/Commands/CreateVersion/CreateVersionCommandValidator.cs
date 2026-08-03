using FluentValidation;

namespace BerexQms.Application.DocumentControl.Commands.CreateVersion;

public sealed class CreateVersionCommandValidator : AbstractValidator<CreateVersionCommand>
{
    public CreateVersionCommandValidator()
    {
        RuleFor(x => x.DocumentId).NotEmpty();
        RuleFor(x => x.VersionNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Content).NotEmpty();
    }
}
