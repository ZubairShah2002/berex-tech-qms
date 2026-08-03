using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.AuditManagement.Commands.StartAudit;

public sealed record StartAuditCommand(
    Guid AuditPlanId,
    Guid AuditRecordId) : ICommand;
