using BerexQms.Domain.Calibration.Enums;
using BerexQms.Domain.Calibration.Events;
using BerexQms.Domain.Calibration.ValueObjects;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Calibration.Entities;

public sealed class Equipment : AggregateRoot<Guid>, IAuditableEntity
{
    private readonly List<CalibrationRecord> _calibrations = [];
    private readonly List<GaugeControl> _gaugeStudies = [];
    private readonly List<ImpactAssessment> _impactAssessments = [];

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Type { get; private set; }
    public string? Manufacturer { get; private set; }
    public string? Model { get; private set; }
    public string? SerialNumber { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public string? Location { get; private set; }
    public EquipmentAssignment? Assignment { get; private set; }
    public CalibrationSchedule? Schedule { get; private set; }

    public IReadOnlyCollection<CalibrationRecord> Calibrations => _calibrations.AsReadOnly();
    public IReadOnlyCollection<GaugeControl> GaugeStudies => _gaugeStudies.AsReadOnly();
    public IReadOnlyCollection<ImpactAssessment> ImpactAssessments => _impactAssessments.AsReadOnly();

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private Equipment() { }

    public static Equipment Create(
        Guid id,
        TenantId tenantId,
        string code,
        string name,
        string? type,
        string? manufacturer,
        string? model,
        string? serialNumber,
        string? location,
        string? department,
        string? area,
        Guid? custodianId)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Equipment code is required.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Equipment name is required.");

        var equipment = new Equipment
        {
            Id = id,
            TenantId = tenantId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Type = type?.Trim(),
            Manufacturer = manufacturer?.Trim(),
            Model = model?.Trim(),
            SerialNumber = serialNumber?.Trim(),
            Status = EquipmentStatus.Active.ToString(),
            Location = location?.Trim(),
        };

        if (!string.IsNullOrWhiteSpace(department))
        {
            equipment.Assignment = new EquipmentAssignment(department.Trim(), area?.Trim(), custodianId);
        }

        return equipment;
    }

    public void UpdateDetails(
        string name,
        string? type,
        string? manufacturer,
        string? model,
        string? serialNumber,
        string? location,
        string? department,
        string? area,
        Guid? custodianId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Equipment name is required.");

        Name = name.Trim();
        Type = type?.Trim();
        Manufacturer = manufacturer?.Trim();
        Model = model?.Trim();
        SerialNumber = serialNumber?.Trim();
        Location = location?.Trim();

        if (!string.IsNullOrWhiteSpace(department))
        {
            Assignment = new EquipmentAssignment(department.Trim(), area?.Trim(), custodianId);
        }
    }

    public CalibrationSchedule SetSchedule(
        int intervalDays,
        int leadTimeDays,
        string labType,
        string? procedureRef,
        DateTime nextDueDate)
    {
        if (Schedule is not null)
        {
            Schedule.UpdateSchedule(intervalDays, leadTimeDays, labType, procedureRef, nextDueDate);
            return Schedule;
        }

        Schedule = CalibrationSchedule.Create(
            Guid.NewGuid(), TenantId, Id, intervalDays, leadTimeDays,
            labType, procedureRef, nextDueDate);

        return Schedule;
    }

    public CalibrationRecord RecordCalibration(
        DateTime calibrationDate,
        CalibrationResult result,
        Guid? technicianId,
        string? procedureRef,
        string? notes,
        string? environmentalConditions)
    {
        var nextDueDate = Schedule is not null
            ? calibrationDate.AddDays(Schedule.IntervalDays)
            : (DateTime?)null;

        var record = CalibrationRecord.Create(
            Guid.NewGuid(), TenantId, Id, calibrationDate, result,
            technicianId, procedureRef, notes, environmentalConditions, nextDueDate);

        _calibrations.Add(record);

        if (result == CalibrationResult.Fail)
        {
            Status = EquipmentStatus.OutOfService.ToString();
            var lastPassDate = GetLastPassCalibrationDate();
            var assessment = ImpactAssessment.Create(
                Guid.NewGuid(), TenantId, Id, record.Id,
                lastPassDate ?? calibrationDate.AddDays(-(Schedule?.IntervalDays ?? 365)),
                calibrationDate, 0);
            _impactAssessments.Add(assessment);
        }
        else
        {
            Status = EquipmentStatus.Active.ToString();
            Schedule?.AdvanceDueDate(calibrationDate);
        }

        AddDomainEvent(new EquipmentCalibratedEvent(
            Id, result.ToString(), record.Id, nextDueDate, TenantId.Value));

        return record;
    }

    public GaugeControl RecordGaugeStudy(
        Guid? characteristicId,
        DateTime studyDate,
        decimal totalGRRPct,
        decimal repeatabilityPct,
        decimal reproducibilityPct,
        decimal? partVariationPct,
        int? ndc)
    {
        var study = GaugeControl.Create(
            Guid.NewGuid(), TenantId, Id, characteristicId, studyDate,
            totalGRRPct, repeatabilityPct, reproducibilityPct, partVariationPct, ndc);

        _gaugeStudies.Add(study);
        return study;
    }

    public void MarkDueForCalibration()
    {
        if (Status == EquipmentStatus.Active.ToString())
        {
            Status = EquipmentStatus.DueForCalibration.ToString();
        }
    }

    public void MarkOverdue()
    {
        if (Status == EquipmentStatus.Active.ToString() ||
            Status == EquipmentStatus.DueForCalibration.ToString())
        {
            Status = EquipmentStatus.Overdue.ToString();
            AddDomainEvent(new Common.Events.CalibrationDueEvent(
                Id, Schedule?.NextDueDate ?? DateTime.UtcNow, GetLastPassCalibrationDate()));
        }
    }

    public void StartCalibration()
    {
        Status = EquipmentStatus.InCalibration.ToString();
    }

    public void Retire()
    {
        Status = EquipmentStatus.Retired.ToString();
    }

    private DateTime? GetLastPassCalibrationDate()
    {
        return _calibrations
            .Where(c => c.Result == CalibrationResult.Pass.ToString() ||
                        c.Result == CalibrationResult.PassWithAdjustment.ToString())
            .OrderByDescending(c => c.CalibrationDate)
            .FirstOrDefault()?.CalibrationDate;
    }
}
