using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Calibration.DTOs;

namespace BerexQms.Application.Calibration.Queries.ListEquipment;

public sealed record ListEquipmentQuery(
    string? SearchTerm,
    string? Status,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<EquipmentDto>>;
