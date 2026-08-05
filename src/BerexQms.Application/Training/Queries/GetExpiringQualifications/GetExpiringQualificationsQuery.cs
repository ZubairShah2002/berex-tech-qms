using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Training.DTOs;

namespace BerexQms.Application.Training.Queries.GetExpiringQualifications;

public sealed record GetExpiringQualificationsQuery(
    int WithinDays = 30) : IQuery<IReadOnlyList<CompetencyRecordDto>>;
