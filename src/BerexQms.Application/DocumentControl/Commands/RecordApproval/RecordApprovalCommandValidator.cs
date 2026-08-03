using FluentValidation;

namespace BerexQms.Application.DocumentControl.Commands.RecordApproval;

public sealed class RecordApprovalCommandValidator : AbstractValidator<RecordApprovalCommand>
{
    public RecordApprovalCommandValidator()
    {
        RuleFor(x => x.DocumentId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
        RuleFor(x => x.Decision).NotEmpty();
    }
}
