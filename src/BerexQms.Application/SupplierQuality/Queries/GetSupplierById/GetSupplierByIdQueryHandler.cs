using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.SupplierQuality.DTOs;
using BerexQms.Domain.SupplierQuality.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.SupplierQuality.Queries.GetSupplierById;

internal sealed class GetSupplierByIdQueryHandler : IQueryHandler<GetSupplierByIdQuery, SupplierDetailDto>
{
    private readonly ISupplierRepository _repository;

    public GetSupplierByIdQueryHandler(ISupplierRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<SupplierDetailDto>> Handle(
        GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        var supplier = await _repository.GetFullDetailAsync(request.SupplierId, cancellationToken);
        if (supplier is null)
            return SupplierErrors.NotFound;

        var approvals = supplier.Approvals.Select(a => new SupplierApprovalDto(
            a.Id, a.ScopeDescription, a.ApprovedDate, a.ExpiryDate, a.Conditions, a.IsActive)).ToList();

        var scorecards = supplier.Scorecards
            .OrderByDescending(sc => sc.PeriodStart)
            .Select(sc => new SupplierScorecardDto(
                sc.Id, sc.PeriodStart, sc.PeriodEnd, sc.QualityScore, sc.DeliveryScore,
                sc.ResponsivenessScore, sc.CostScore, sc.OverallScore, sc.Status)).ToList();

        var scars = supplier.Scars
            .OrderByDescending(s => s.IssuedDate)
            .Select(s => new SCARRecordDto(
                s.Id, s.ScarNumber, s.NonConformanceId, s.DefectDescription, s.Severity,
                s.IssuedDate, s.ResponseDeadline, s.Status,
                s.Response?.RootCause, s.Response?.CorrectiveActions,
                s.Response?.EvidenceRefs, s.Response?.ResponseDate)).ToList();

        var approvedParts = supplier.ApprovedParts.Select(ap => new ApprovedPartDto(
            ap.Id, ap.PartId, ap.RevisionScope, ap.ApprovalDate, ap.IsActive)).ToList();

        return new SupplierDetailDto(
            supplier.Id,
            supplier.Code,
            supplier.Name,
            supplier.Status,
            supplier.RiskLevel,
            supplier.Tier,
            supplier.ApprovedSince,
            supplier.PrimaryContact?.Name,
            supplier.PrimaryContact?.Role,
            supplier.PrimaryContact?.Email,
            supplier.PrimaryContact?.Phone,
            supplier.RiskAssessment?.Level.ToString(),
            supplier.RiskAssessment?.ContributingFactors,
            supplier.RiskAssessment?.AssessedAt,
            approvals,
            scorecards,
            scars,
            approvedParts,
            supplier.CreatedAt,
            supplier.CreatedBy,
            supplier.ModifiedAt);
    }
}
