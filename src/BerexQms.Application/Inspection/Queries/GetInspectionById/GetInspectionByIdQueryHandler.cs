using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Inspection.DTOs;
using BerexQms.Domain.Inspection.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Inspection.Queries.GetInspectionById;

public sealed class GetInspectionByIdQueryHandler
    : IQueryHandler<GetInspectionByIdQuery, InspectionDetailDto>
{
    private readonly IInspectionRepository _inspectionRepository;

    public GetInspectionByIdQueryHandler(IInspectionRepository inspectionRepository)
    {
        _inspectionRepository = inspectionRepository;
    }

    public async Task<Result<InspectionDetailDto>> Handle(
        GetInspectionByIdQuery request, CancellationToken cancellationToken)
    {
        var record = await _inspectionRepository.GetFullDetailAsync(
            request.InspectionId, cancellationToken);
        if (record is null)
            return InspectionErrors.NotFound;

        var checklist = record.Checklist is not null
            ? new ChecklistDto(
                record.Checklist.Id,
                record.Checklist.PartRevisionId,
                record.Checklist.RevisionCode,
                record.Checklist.SnapshotAt,
                record.Checklist.Items.Select(i => new ChecklistItemDto(
                    i.Id,
                    i.CharacteristicName,
                    i.SpecificationLimit,
                    i.NominalValue,
                    i.UpperLimit,
                    i.LowerLimit,
                    i.Unit,
                    i.IsCritical,
                    i.SortOrder)).ToList())
            : null;

        var gateResults = record.GateResults.Select(g => new GateResultDto(
            g.GateType.ToString(),
            g.Passed,
            g.Detail,
            g.CheckedAt)).ToList();

        var measurements = record.Measurements.Select(m => new MeasurementDto(
            m.Id,
            m.ChecklistItemId,
            m.CharacteristicName,
            m.MeasuredValue,
            m.TextValue,
            m.Unit,
            m.Result.ToString(),
            m.EquipmentId,
            m.OperatorId,
            m.RecordedAt,
            m.SequenceNumber)).ToList();

        var disposition = record.Disposition is not null
            ? new DispositionDto(
                record.Disposition.Type.ToString(),
                record.Disposition.Justification,
                record.Disposition.ApprovedBy,
                record.Disposition.ApprovedAt)
            : null;

        return new InspectionDetailDto(
            record.Id,
            record.InspectionNumber,
            record.Type.ToString(),
            record.Status.ToString(),
            record.PartId,
            record.PartRevisionId,
            record.LotNumber,
            record.LotSize,
            record.SampleSize,
            record.SupplierId,
            record.SamplingPlanId,
            record.InspectorId,
            record.Result?.ToString(),
            record.Notes,
            record.CompletedAt,
            record.CompletedBy,
            record.ApprovedAt,
            record.ApprovedBy,
            disposition,
            checklist,
            gateResults,
            measurements,
            record.CreatedAt,
            record.CreatedBy,
            record.ModifiedAt);
    }
}
