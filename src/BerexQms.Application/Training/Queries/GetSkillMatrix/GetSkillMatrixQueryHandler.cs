using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Training.DTOs;
using BerexQms.Domain.Training.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Training.Queries.GetSkillMatrix;

internal sealed class GetSkillMatrixQueryHandler
    : IQueryHandler<GetSkillMatrixQuery, IReadOnlyList<SkillMatrixEntryDto>>
{
    private readonly ICompetencyRecordRepository _competencyRepository;
    private readonly IQualificationRepository _qualificationRepository;

    public GetSkillMatrixQueryHandler(
        ICompetencyRecordRepository competencyRepository,
        IQualificationRepository qualificationRepository)
    {
        _competencyRepository = competencyRepository;
        _qualificationRepository = qualificationRepository;
    }

    public async Task<Result<IReadOnlyList<SkillMatrixEntryDto>>> Handle(
        GetSkillMatrixQuery request, CancellationToken cancellationToken)
    {
        var records = await _competencyRepository.GetAllForSkillMatrixAsync(
            request.Department, request.ProductFamily, cancellationToken);

        // Batch-load all referenced qualifications in a single query
        var qualificationIds = records.Select(r => r.QualificationId).Distinct();
        var qualificationList = await _qualificationRepository.GetByIdsAsync(qualificationIds, cancellationToken);
        var qualifications = qualificationList.ToDictionary(q => q.Id, q => (q.Code, q.Name));

        var entries = records.Select(r =>
        {
            var (code, name) = qualifications.GetValueOrDefault(r.QualificationId, ("Unknown", "Unknown"));
            return new SkillMatrixEntryDto(
                r.EmployeeId,
                r.QualificationId,
                code,
                name,
                r.Status,
                r.ExpiryDate);
        }).ToList();

        return entries;
    }
}
