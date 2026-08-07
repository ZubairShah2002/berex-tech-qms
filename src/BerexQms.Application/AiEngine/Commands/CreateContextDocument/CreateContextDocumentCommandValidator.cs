using BerexQms.Domain.AiEngine.Enums;
using FluentValidation;

namespace BerexQms.Application.AiEngine.Commands.CreateContextDocument;

public sealed class CreateContextDocumentCommandValidator
    : AbstractValidator<CreateContextDocumentCommand>
{
    public CreateContextDocumentCommandValidator()
    {
        RuleFor(x => x.SourceModule)
            .NotEmpty().WithMessage("Source module is required.")
            .MaximumLength(100).WithMessage("Source module cannot exceed 100 characters.");

        RuleFor(x => x.SourceEntityId)
            .MaximumLength(200).WithMessage("Source entity ID cannot exceed 200 characters.");

        RuleFor(x => x.ContextType)
            .NotEmpty().WithMessage("Context type is required.")
            .Must(v => Enum.TryParse<AiContextType>(v, true, out _))
            .WithMessage("Invalid context type. Valid values: Product, Quality, Supplier, " +
                         "Document, NonConformance, CorrectiveAction, Audit, Calibration, " +
                         "Training, StatisticalProcess.");

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
