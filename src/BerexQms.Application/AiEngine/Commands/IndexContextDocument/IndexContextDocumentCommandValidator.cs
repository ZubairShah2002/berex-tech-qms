using FluentValidation;

namespace BerexQms.Application.AiEngine.Commands.IndexContextDocument;

public sealed class IndexContextDocumentCommandValidator
    : AbstractValidator<IndexContextDocumentCommand>
{
    public IndexContextDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("Document ID is required.");
    }
}
