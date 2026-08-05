using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Training.DTOs;
using BerexQms.Domain.Training.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Training.Queries.GetEmployeeCompetencies;

internal sealed class GetEmployeeCompetenciesQueryHandler
    : IQueryHandler<GetEmployeeCompetenciesQuery, IReadOnlyList<CompetencyRecordDto>>
{
    private readonly ICompetencyRecordRepository _repository;

    public GetEmployeeCompetenciesQueryHandler(ICompetencyRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<CompetencyRecordDto>>> Handle(
        GetEmployeeCompetenciesQuery request, CancellationToken cancellationToken)
    {
        var records = await _repository.GetByEmployeeAsync(request.EmployeeId, cancellationToken);

        var dtos = records.Select(r => new CompetencyRecordDto(
            r.Id,
            r.EmployeeId,
            r.QualificationId,
            r.Status,
            r.QualifiedDate,
            r.ExpiryDate,
            r.AssessorId,
            r.EvidenceRef)).ToList();

        return dtos;
    }
}
