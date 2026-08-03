using FluentValidation;

namespace BerexQms.Application.DocumentControl.Commands.CreateDocument;

public sealed class CreateDocumentCommandValidator : AbstractValidator<CreateDocumentCommand>
{
    public CreateDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DocumentType).NotEmpty();
    }
}
