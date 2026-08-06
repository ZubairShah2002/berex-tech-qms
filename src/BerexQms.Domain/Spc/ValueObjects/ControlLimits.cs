namespace BerexQms.Domain.Spc.ValueObjects;

public sealed record ControlLimits(
    decimal UpperControlLimit,
    decimal CenterLine,
    decimal LowerControlLimit,
    decimal? UpperSpecLimit,
    decimal? LowerSpecLimit);
