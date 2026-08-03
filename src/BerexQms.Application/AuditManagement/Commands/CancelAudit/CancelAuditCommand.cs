using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.AuditManagement.Commands.CancelAudit;

public sealed record CancelAuditCommand(
    Guid AuditPlanId,
    Guid AuditRecordId) : ICommand;
