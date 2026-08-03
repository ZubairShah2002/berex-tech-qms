using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.AuditManagement.Commands.CreateAuditPlan;

public sealed record CreateAuditPlanCommand(
    string PlanName,
    int Year,
    string? Description,
    string? Scope) : ICommand<Guid>;
