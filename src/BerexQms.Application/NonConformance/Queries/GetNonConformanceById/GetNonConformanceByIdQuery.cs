using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.NonConformance.DTOs;

namespace BerexQms.Application.NonConformance.Queries.GetNonConformanceById;

public sealed record GetNonConformanceByIdQuery(Guid NonConformanceId) : IQuery<NonConformanceDetailDto>;
