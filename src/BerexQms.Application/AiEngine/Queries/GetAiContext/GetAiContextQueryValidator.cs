using FluentValidation;

namespace BerexQms.Application.AiEngine.Queries.GetAiContext;

public sealed class GetAiContextQueryValidator : AbstractValidator<GetAiContextQuery>
{
    public GetAiContextQueryValidator()
    {
        RuleFor(x => x.SourceModule)
            .NotEmpty().WithMessage("Source module is required.")
            .MaximumLength(100).WithMessage("Source module cannot exceed 100 characters.");
    }
}
