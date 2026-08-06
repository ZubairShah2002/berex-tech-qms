namespace BerexQms.Domain.Spc.ValueObjects;

public sealed record ProcessCapability(
    decimal Cp,
    decimal Cpk,
    decimal Pp,
    decimal Ppk,
    decimal Mean,
    decimal StdDev,
    int SampleSize,
    DateTime CalculatedAt);
