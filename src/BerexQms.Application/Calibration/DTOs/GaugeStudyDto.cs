namespace BerexQms.Application.Calibration.DTOs;

public sealed record GaugeStudyDto(
    Guid Id,
    Guid? CharacteristicId,
    DateTime StudyDate,
    decimal TotalGRRPct,
    decimal RepeatabilityPct,
    decimal ReproducibilityPct,
    decimal? PartVariationPct,
    int? Ndc,
    string Result);
