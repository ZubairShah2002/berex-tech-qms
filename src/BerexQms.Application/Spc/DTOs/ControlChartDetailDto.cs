namespace BerexQms.Application.Spc.DTOs;

public sealed record ControlChartDetailDto(
    Guid Id,
    string Code,
    string Name,
    string ChartType,
    Guid PartId,
    string CharacteristicName,
    int SubgroupSize,
    string Status,
    bool IsActive,
    ControlLimitsDto? ControlLimits,
    ProcessCapabilityDto? ProcessCapability,
    decimal? UpperSpecLimit,
    decimal? LowerSpecLimit,
    IReadOnlyList<DataPointDto> DataPoints,
    DateTime CreatedAt);
