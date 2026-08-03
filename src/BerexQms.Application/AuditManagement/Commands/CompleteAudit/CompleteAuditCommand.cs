using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.AuditManagement.Commands.CompleteAudit;

public sealed record CompleteAuditCommand(
    Guid AuditPlanId,
    Guid AuditRecordId,
    string Summary,
    string Recommendations,
    string? AuditorNotes) : ICommand;
