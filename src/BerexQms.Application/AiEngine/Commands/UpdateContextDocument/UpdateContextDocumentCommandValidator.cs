using FluentValidation;

namespace BerexQms.Application.AiEngine.Commands.UpdateContextDocument;

public sealed class UpdateContextDocumentCommandValidator
    : AbstractValidator<UpdateContextDocumentCommand>
{
    public UpdateContextDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("Document ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(500).WithMessage("Title cannot exceed 500 characters.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(100_000).WithMessage("Content cannot exceed 100,000 characters.");

        RuleFor(x => x.MetadataJson)
            .MaximumLength(10_000).WithMessage("Metadata JSON cannot exceed 10,000 characters.");
    }
}
