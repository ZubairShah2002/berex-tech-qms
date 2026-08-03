using FluentValidation;

namespace BerexQms.Application.DocumentControl.Commands.StartApproval;

public sealed class StartApprovalCommandValidator : AbstractValidator<StartApprovalCommand>
{
    public StartApprovalCommandValidator()
    {
        RuleFor(x => x.DocumentId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
        RuleFor(x => x.ApproverIds).NotEmpty();
    }
}
