using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AuditManagement.DTOs;

namespace BerexQms.Application.AuditManagement.Commands.RecordFinding;

public sealed record RecordFindingCommand(
    Guid AuditPlanId,
    Guid AuditRecordId,
    string Classification,
    string ClauseReference,
    string Description,
    string? Evidence,
    string? CorrectiveAction,
    string? LinkedCapaId) : ICommand<AuditFindingDto>;
