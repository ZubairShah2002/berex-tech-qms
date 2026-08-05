using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Training.DTOs;

namespace BerexQms.Application.Training.Queries.ListQualifications;

public sealed record ListQualificationsQuery(
    string? SearchTerm,
    bool? IsActive,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<QualificationDto>>;
