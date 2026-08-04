using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Calibration.DTOs;

namespace BerexQms.Application.Calibration.Commands.RecordGaugeStudy;

public sealed record RecordGaugeStudyCommand(
    Guid EquipmentId,
    Guid? CharacteristicId,
    DateTime StudyDate,
    decimal TotalGRRPct,
    decimal RepeatabilityPct,
    decimal ReproducibilityPct,
    decimal? PartVariationPct,
    int? Ndc) : ICommand<GaugeStudyDto>;
