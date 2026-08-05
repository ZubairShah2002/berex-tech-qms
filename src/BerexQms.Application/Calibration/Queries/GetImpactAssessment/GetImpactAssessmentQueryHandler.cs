using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Calibration.DTOs;
using BerexQms.Domain.Calibration.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Calibration.Queries.GetImpactAssessment;

internal sealed class GetImpactAssessmentQueryHandler
    : IQueryHandler<GetImpactAssessmentQuery, ImpactAssessmentDto>
{
    private readonly IEquipmentRepository _repository;

    public GetImpactAssessmentQueryHandler(IEquipmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ImpactAssessmentDto>> Handle(
        GetImpactAssessmentQuery request, CancellationToken cancellationToken)
    {
        var assessment = await _repository.GetImpactAssessmentByIdAsync(
            request.AssessmentId, cancellationToken);
        if (assessment is null)
            return CalibrationErrors.AssessmentNotFound;

        return new ImpactAssessmentDto(
            assessment.Id,
            assessment.EquipmentId,
            assessment.FailedCalibrationId,
            assessment.AffectedFrom,
            assessment.AffectedTo,
            assessment.AffectedInspectionCount,
            assessment.Status,
            assessment.ReviewedBy,
            assessment.Notes);
    }
}
