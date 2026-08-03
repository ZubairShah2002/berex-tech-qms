using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AuditManagement.DTOs;

namespace BerexQms.Application.AuditManagement.Commands.AddChecklist;

public sealed record AddChecklistCommand(
    Guid AuditPlanId,
    Guid AuditRecordId,
    string Standard,
    string ClauseReference,
    string Requirement,
    bool IsCompliant,
    string? Evidence,
    string? Notes) : ICommand<AuditChecklistDto>;
