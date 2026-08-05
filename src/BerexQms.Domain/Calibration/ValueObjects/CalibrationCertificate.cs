namespace BerexQms.Domain.Calibration.ValueObjects;

public sealed record CalibrationCertificate(
    string IssuingLab,
    string? AccreditationRef,
    string? FileRef,
    DateTime ValidFrom,
    DateTime ValidUntil);
