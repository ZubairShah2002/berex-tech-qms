using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.NonConformance.DTOs;
using BerexQms.Domain.NonConformance.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.NonConformance.Queries.GetNonConformanceById;

public sealed class GetNonConformanceByIdQueryHandler
    : IQueryHandler<GetNonConformanceByIdQuery, NonConformanceDetailDto>
{
    private readonly INonConformanceRepository _repository;

    public GetNonConformanceByIdQueryHandler(INonConformanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<NonConformanceDetailDto>> Handle(
        GetNonConformanceByIdQuery request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetFullDetailAsync(request.NonConformanceId, cancellationToken);
        if (record is null)
            return NonConformanceErrors.NotFound;

        var classification = record.Classification is not null
            ? new ClassificationDto(
                record.Classification.Category,
                record.Classification.DefectType,
                record.Classification.DefectCode)
            : null;

        var disposition = record.Disposition is not null
            ? new NCDispositionDto(
                record.Disposition.Type.ToString(),
                record.Disposition.Justification,
                record.Disposition.ApprovedBy,
                record.Disposition.ApprovedAt)
            : null;

        var impact = record.ImpactAssessment is not null
            ? new ImpactAssessmentDto(
                record.ImpactAssessment.AffectedQuantity,
                record.ImpactAssessment.ShippedProductAffected,
                record.ImpactAssessment.CustomerImpactDescription)
            : null;

        var containmentActions = record.ContainmentActions.Select(a => new ContainmentActionDto(
            a.Id,
            a.Description,
            a.ActionTakenBy,
            a.ActionTakenAt,
            a.IsVerified,
            a.VerifiedBy,
            a.VerifiedAt)).ToList();

        var investigations = record.Investigations.Select(i => new InvestigationDto(
            i.Id,
            i.InvestigatorId,
            i.Methodology,
            i.RootCause,
            i.Findings,
            i.StartedAt,
            i.CompletedAt)).ToList();

        return new NonConformanceDetailDto(
            record.Id,
            record.NcrNumber,
            record.Status.ToString(),
            record.Severity.ToString(),
            record.Source.ToString(),
            record.DetectionPoint.ToString(),
            record.Description,
            record.PartId,
            record.PartRevisionId,
            record.LotNumber,
            record.SerialNumber,
            record.SupplierId,
            record.SupplierLotNumber,
            record.WorkOrderNumber,
            record.CustomerId,
            record.SourceInspectionId,
            record.QuantityAffected,
            record.QuantityDefective,
            classification,
            disposition,
            impact,
            record.AssignedTo,
            record.CapaId,
            record.ClosedAt,
            record.ClosedBy,
            record.ReopenedAt,
            record.ReopenedBy,
            record.ReopenReason,
            record.ClosureNotes,
            containmentActions,
            investigations,
            record.CreatedAt,
            record.CreatedBy,
            record.ModifiedAt);
    }
}
