namespace BerexQms.Application.Spc.DTOs;

public sealed record ControlChartDto(
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
    int DataPointCount,
    DateTime CreatedAt);
