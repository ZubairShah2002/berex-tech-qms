using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Training.DTOs;
using BerexQms.Domain.Training.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Training.Queries.GetQualification;

internal sealed class GetQualificationQueryHandler
    : IQueryHandler<GetQualificationQuery, QualificationDto>
{
    private readonly IQualificationRepository _repository;

    public GetQualificationQueryHandler(IQualificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<QualificationDto>> Handle(
        GetQualificationQuery request, CancellationToken cancellationToken)
    {
        var qualification = await _repository.GetByIdAsync(request.QualificationId, cancellationToken);
        if (qualification is null)
            return TrainingErrors.QualificationNotFound;

        return new QualificationDto(
            qualification.Id,
            qualification.Code,
            qualification.Name,
            qualification.Description,
            qualification.ScopeProductFamily,
            qualification.ScopeInspectionType,
            qualification.ScopeProcessArea,
            qualification.ValidityMonths,
            qualification.RenewalWindowDays,
            qualification.IsActive,
            qualification.CreatedAt);
    }
}
