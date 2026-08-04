using BerexQms.Domain.Calibration.Enums;
using BerexQms.Domain.Calibration.ValueObjects;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Calibration.Entities;

public sealed class CalibrationRecord : Entity<Guid>
{
    public Guid EquipmentId { get; private set; }
    public DateTime CalibrationDate { get; private set; }
    public string Result { get; private set; } = string.Empty;
    public Guid? TechnicianId { get; private set; }
    public string? ProcedureRef { get; private set; }
    public string? Notes { get; private set; }
    public string? EnvironmentalConditions { get; private set; }
    public DateTime? NextDueDate { get; private set; }
    public CalibrationCertificate? Certificate { get; private set; }

    private CalibrationRecord() { }

    internal static CalibrationRecord Create(
        Guid id,
        TenantId tenantId,
        Guid equipmentId,
        DateTime calibrationDate,
        CalibrationResult result,
        Guid? technicianId,
        string? procedureRef,
        string? notes,
        string? environmentalConditions,
        DateTime? nextDueDate)
    {
        return new CalibrationRecord
        {
            Id = id,
            TenantId = tenantId,
            EquipmentId = equipmentId,
            CalibrationDate = calibrationDate,
            Result = result.ToString(),
            TechnicianId = technicianId,
            ProcedureRef = procedureRef?.Trim(),
            Notes = notes?.Trim(),
            EnvironmentalConditions = environmentalConditions?.Trim(),
            NextDueDate = nextDueDate,
        };
    }

    public void AttachCertificate(
        string issuingLab,
        string? accreditationRef,
        string? fileRef,
        DateTime validFrom,
        DateTime validUntil)
    {
        if (string.IsNullOrWhiteSpace(issuingLab))
            throw new DomainException("Issuing laboratory is required for certificate.");

        Certificate = new CalibrationCertificate(
            issuingLab.Trim(), accreditationRef?.Trim(), fileRef?.Trim(), validFrom, validUntil);
    }
}
