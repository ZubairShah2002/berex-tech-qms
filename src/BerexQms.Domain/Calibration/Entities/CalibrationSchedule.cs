using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Calibration.Entities;

public sealed class CalibrationSchedule : Entity<Guid>
{
    public Guid EquipmentId { get; private set; }
    public int IntervalDays { get; private set; }
    public int LeadTimeDays { get; private set; }
    public string LabType { get; private set; } = string.Empty;
    public string? ProcedureRef { get; private set; }
    public DateTime NextDueDate { get; private set; }

    private CalibrationSchedule() { }

    internal static CalibrationSchedule Create(
        Guid id,
        TenantId tenantId,
        Guid equipmentId,
        int intervalDays,
        int leadTimeDays,
        string labType,
        string? procedureRef,
        DateTime nextDueDate)
    {
        if (intervalDays <= 0)
            throw new DomainException("Calibration interval must be greater than zero.");
        if (leadTimeDays < 0)
            throw new DomainException("Lead time cannot be negative.");
        if (string.IsNullOrWhiteSpace(labType))
            throw new DomainException("Laboratory type is required.");

        return new CalibrationSchedule
        {
            Id = id,
            TenantId = tenantId,
            EquipmentId = equipmentId,
            IntervalDays = intervalDays,
            LeadTimeDays = leadTimeDays,
            LabType = labType.Trim(),
            ProcedureRef = procedureRef?.Trim(),
            NextDueDate = nextDueDate,
        };
    }

    public void UpdateSchedule(int intervalDays, int leadTimeDays, string labType, string? procedureRef, DateTime nextDueDate)
    {
        if (intervalDays <= 0)
            throw new DomainException("Calibration interval must be greater than zero.");
        if (leadTimeDays < 0)
            throw new DomainException("Lead time cannot be negative.");

        IntervalDays = intervalDays;
        LeadTimeDays = leadTimeDays;
        LabType = labType.Trim();
        ProcedureRef = procedureRef?.Trim();
        NextDueDate = nextDueDate;
    }

    public void AdvanceDueDate(DateTime calibrationDate)
    {
        NextDueDate = calibrationDate.AddDays(IntervalDays);
    }
}
