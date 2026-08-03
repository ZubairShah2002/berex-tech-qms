using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AuditManagement.DTOs;

namespace BerexQms.Application.AuditManagement.Queries.GetAuditPlanById;

public sealed record GetAuditPlanByIdQuery(Guid AuditPlanId) : IQuery<AuditPlanDetailDto>;
