using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Calibration.Commands.AttachCertificate;

public sealed record AttachCertificateCommand(
    Guid EquipmentId,
    Guid CalibrationId,
    string IssuingLab,
    string? AccreditationRef,
    string? FileRef,
    DateTime ValidFrom,
    DateTime ValidUntil) : ICommand;
