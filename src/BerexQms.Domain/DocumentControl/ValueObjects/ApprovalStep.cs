using BerexQms.Domain.DocumentControl.Enums;

namespace BerexQms.Domain.DocumentControl.ValueObjects;

public sealed record ApprovalStep(
    int StepOrder,
    string ApproverId,
    ApprovalDecision? Decision,
    string? Comments,
    string? Signature,
    DateTime? DecidedAt);
