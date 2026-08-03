using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Capa.DTOs;
using BerexQms.Domain.Capa.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Capa.Queries.GetCapaById;

public sealed class GetCapaByIdQueryHandler : IQueryHandler<GetCapaByIdQuery, CAPADetailDto>
{
    private readonly ICAPARepository _repository;

    public GetCapaByIdQueryHandler(ICAPARepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CAPADetailDto>> Handle(
        GetCapaByIdQuery request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetFullDetailAsync(request.CapaId, cancellationToken);
        if (record is null)
            return CAPAErrors.NotFound;

        var source = new CAPASourceDto(
            record.Source.SourceType.ToString(),
            record.Source.SourceNonConformanceId,
            record.Source.SourceAuditFindingId,
            record.Source.SourceDescription);

        var rca = record.RootCauseAnalysis is not null
            ? new RootCauseAnalysisDto(
                record.RootCauseAnalysis.Id,
                record.RootCauseAnalysis.Methodology.ToString(),
                record.RootCauseAnalysis.AnalysisDetails,
                record.RootCauseAnalysis.RootCause,
                record.RootCauseAnalysis.ContributingFactors,
                record.RootCauseAnalysis.AnalystId,
                record.RootCauseAnalysis.StartedAt,
                record.RootCauseAnalysis.CompletedAt)
            : null;

        var actions = record.Actions.Select(a => new CapaActionDto(
            a.Id,
            a.ActionType.ToString(),
            a.Description,
            a.OwnerId,
            a.DueDate,
            a.EvidenceRequirement,
            a.CompletionNotes,
            a.EvidenceProvided,
            a.CompletedAt,
            a.CompletedBy,
            a.IsOverdue,
            a.CreatedAt)).ToList();

        var verifications = record.Verifications.Select(v => new EffectivenessVerificationDto(
            v.Id,
            v.ScheduledDate,
            v.VerificationCriteria,
            v.VerifierId,
            v.Result,
            v.Evidence,
            v.IsEffective,
            v.VerifiedAt,
            v.CreatedAt)).ToList();

        return new CAPADetailDto(
            record.Id,
            record.CapaNumber,
            record.Title,
            record.Description,
            record.Status.ToString(),
            record.Priority.ToString(),
            source,
            record.OwnerId,
            record.AssignedTo,
            record.SourceNonConformanceId,
            record.TargetClosureDate,
            record.ClosedAt,
            record.ClosedBy,
            record.ClosureNotes,
            rca,
            actions,
            verifications,
            record.CreatedAt,
            record.CreatedBy,
            record.ModifiedAt);
    }
}
