namespace BerexQms.Application.Spc.DTOs;

public sealed record ProcessCapabilityDto(
    decimal Cp,
    decimal Cpk,
    decimal Pp,
    decimal Ppk,
    decimal Mean,
    decimal StdDev,
    int SampleSize,
    DateTime CalculatedAt);
