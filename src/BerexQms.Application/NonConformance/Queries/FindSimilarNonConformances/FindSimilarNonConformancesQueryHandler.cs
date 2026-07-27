using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.NonConformance.DTOs;
using BerexQms.Domain.NonConformance.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.NonConformance.Queries.FindSimilarNonConformances;

public sealed class FindSimilarNonConformancesQueryHandler
    : IQueryHandler<FindSimilarNonConformancesQuery, IReadOnlyList<SimilarNcDto>>
{
    private readonly INonConformanceRepository _repository;

    public FindSimilarNonConformancesQueryHandler(INonConformanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<SimilarNcDto>>> Handle(
        FindSimilarNonConformancesQuery request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(request.NonConformanceId, cancellationToken);
        if (record is null)
            return NonConformanceErrors.NotFound;

        var lookbackFrom = DateTime.UtcNow.AddDays(-request.LookbackDays);

        var records = await _repository.FindSimilarAsync(
            record.PartId, record.Classification?.DefectType, record.SupplierId,
            lookbackFrom, cancellationToken);

        var dtos = records
            .Where(r => r.Id != request.NonConformanceId)
            .Select(r => new SimilarNcDto(
                r.Id,
                r.NcrNumber,
                r.Status.ToString(),
                r.Severity.ToString(),
                r.Classification?.DefectType,
                r.PartId,
                r.SupplierId,
                r.CreatedAt)).ToList();

        return dtos;
    }
}
