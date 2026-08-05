namespace BerexQms.Application.Spc.DTOs;

public sealed record ControlLimitsDto(
    decimal UpperControlLimit,
    decimal CenterLine,
    decimal LowerControlLimit,
    decimal? UpperSpecLimit,
    decimal? LowerSpecLimit);
