using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Calibration.DTOs;
using BerexQms.Domain.Calibration.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Calibration.Queries.GetEquipmentById;

internal sealed class GetEquipmentByIdQueryHandler
    : IQueryHandler<GetEquipmentByIdQuery, EquipmentDetailDto>
{
    private readonly IEquipmentRepository _repository;

    public GetEquipmentByIdQueryHandler(IEquipmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<EquipmentDetailDto>> Handle(
        GetEquipmentByIdQuery request, CancellationToken cancellationToken)
    {
        var equipment = await _repository.GetFullDetailAsync(request.EquipmentId, cancellationToken);
        if (equipment is null)
            return CalibrationErrors.EquipmentNotFound;

        CalibrationScheduleDto? scheduleDto = null;
        if (equipment.Schedule is not null)
        {
            var s = equipment.Schedule;
            scheduleDto = new CalibrationScheduleDto(
                s.Id, s.IntervalDays, s.LeadTimeDays, s.LabType, s.ProcedureRef, s.NextDueDate);
        }

        var calibrations = equipment.Calibrations
            .OrderByDescending(c => c.CalibrationDate)
            .Select(c =>
            {
                CertificateDto? certDto = null;
                if (c.Certificate is not null)
                {
                    var cert = c.Certificate;
                    certDto = new CertificateDto(
                        cert.IssuingLab, cert.AccreditationRef, cert.FileRef,
                        cert.ValidFrom, cert.ValidUntil);
                }

                return new CalibrationRecordDto(
                    c.Id, c.CalibrationDate, c.Result, c.TechnicianId,
                    c.ProcedureRef, c.Notes, c.EnvironmentalConditions,
                    c.NextDueDate, certDto);
            }).ToList();

        var gaugeStudies = equipment.GaugeStudies
            .OrderByDescending(g => g.StudyDate)
            .Select(g => new GaugeStudyDto(
                g.Id, g.CharacteristicId, g.StudyDate,
                g.TotalGRRPct, g.RepeatabilityPct, g.ReproducibilityPct,
                g.PartVariationPct, g.Ndc, g.Result)).ToList();

        var assessments = equipment.ImpactAssessments
            .OrderByDescending(a => a.AffectedTo)
            .Select(a => new ImpactAssessmentDto(
                a.Id, a.EquipmentId, a.FailedCalibrationId,
                a.AffectedFrom, a.AffectedTo, a.AffectedInspectionCount,
                a.Status, a.ReviewedBy, a.Notes)).ToList();

        return new EquipmentDetailDto(
            equipment.Id,
            equipment.Code,
            equipment.Name,
            equipment.Type,
            equipment.Manufacturer,
            equipment.Model,
            equipment.SerialNumber,
            equipment.Status,
            equipment.Location,
            equipment.Assignment?.Department,
            equipment.Assignment?.Area,
            equipment.Assignment?.CustodianId,
            scheduleDto,
            calibrations,
            gaugeStudies,
            assessments,
            equipment.CreatedAt);
    }
}
