using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Training.DTOs;
using BerexQms.Domain.Training.Enums;
using BerexQms.Domain.Training.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Training.Queries.ValidateQualification;

internal sealed class ValidateQualificationQueryHandler
    : IQueryHandler<ValidateQualificationQuery, QualificationValidationDto>
{
    private readonly ICompetencyRecordRepository _competencyRepository;
    private readonly IQualificationRepository _qualificationRepository;

    public ValidateQualificationQueryHandler(
        ICompetencyRecordRepository competencyRepository,
        IQualificationRepository qualificationRepository)
    {
        _competencyRepository = competencyRepository;
        _qualificationRepository = qualificationRepository;
    }

    public async Task<Result<QualificationValidationDto>> Handle(
        ValidateQualificationQuery request, CancellationToken cancellationToken)
    {
        var qualification = await _qualificationRepository.GetByIdAsync(
            request.QualificationId, cancellationToken);
        if (qualification is null)
            return TrainingErrors.QualificationNotFound;

        var record = await _competencyRepository.GetByEmployeeAndQualificationAsync(
            request.EmployeeId, request.QualificationId, cancellationToken);

        var isQualified = record is not null &&
            record.Status == QualificationStatus.Qualified.ToString() &&
            (record.ExpiryDate is null || record.ExpiryDate > DateTime.UtcNow);

        return new QualificationValidationDto(
            isQualified,
            record?.ExpiryDate,
            qualification.Code);
    }
}
