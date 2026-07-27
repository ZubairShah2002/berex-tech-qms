using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.NonConformance.DTOs;

namespace BerexQms.Application.NonConformance.Queries.FindSimilarNonConformances;

public sealed record FindSimilarNonConformancesQuery(
    Guid NonConformanceId,
    int LookbackDays = 90) : IQuery<IReadOnlyList<SimilarNcDto>>;
