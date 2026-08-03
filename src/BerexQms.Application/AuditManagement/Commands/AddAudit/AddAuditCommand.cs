using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AuditManagement.DTOs;

namespace BerexQms.Application.AuditManagement.Commands.AddAudit;

public sealed record AddAuditCommand(
    Guid AuditPlanId,
    string AuditNumber,
    string AuditType,
    string LeadAuditorId,
    string? AuditeeArea,
    DateTime ScheduledDate) : ICommand<AuditRecordDto>;
