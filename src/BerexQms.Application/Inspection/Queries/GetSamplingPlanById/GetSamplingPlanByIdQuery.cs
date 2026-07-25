using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Inspection.DTOs;

namespace BerexQms.Application.Inspection.Queries.GetSamplingPlanById;

public sealed record GetSamplingPlanByIdQuery(Guid SamplingPlanId) : IQuery<SamplingPlanDto>;
