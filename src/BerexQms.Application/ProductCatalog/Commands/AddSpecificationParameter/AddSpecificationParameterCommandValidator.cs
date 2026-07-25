using BerexQms.Domain.ProductCatalog.Enums;
using FluentValidation;

namespace BerexQms.Application.ProductCatalog.Commands.AddSpecificationParameter;

public sealed class AddSpecificationParameterCommandValidator
    : AbstractValidator<AddSpecificationParameterCommand>
{
    public AddSpecificationParameterCommandValidator()
    {
        RuleFor(x => x.PartId)
            .NotEmpty().WithMessage("Part ID is required.");

        RuleFor(x => x.RevisionId)
            .NotEmpty().WithMessage("Revision ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Parameter name is required.")
            .MaximumLength(200).WithMessage("Parameter name cannot exceed 200 characters.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Parameter type is required.")
            .Must(v => Enum.TryParse<ParameterType>(v, true, out _))
            .WithMessage("Invalid parameter type.");

        RuleFor(x => x.Unit)
            .MaximumLength(50).WithMessage("Unit cannot exceed 50 characters.")
            .When(x => x.Unit is not null);

        RuleFor(x => x.TextValue)
            .MaximumLength(500).WithMessage("Text value cannot exceed 500 characters.")
            .When(x => x.TextValue is not null);
    }
}
