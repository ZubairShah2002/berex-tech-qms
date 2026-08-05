using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Spc.DTOs;
using BerexQms.Application.Spc.Queries.ListCharts;
using BerexQms.Domain.Spc.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Spc.Queries.GetChartsByPart;

internal sealed class GetChartsByPartQueryHandler : IQueryHandler<GetChartsByPartQuery, IReadOnlyList<ControlChartDto>>
{
    private readonly IControlChartRepository _repository;

    public GetChartsByPartQueryHandler(IControlChartRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<ControlChartDto>>> Handle(
        GetChartsByPartQuery request, CancellationToken cancellationToken)
    {
        var charts = await _repository.GetByPartIdAsync(request.PartId, cancellationToken);

        var dtos = charts.Select(ListChartsQueryHandler.MapToDto).ToList();

        return Result<IReadOnlyList<ControlChartDto>>.Success(dtos);
    }
}
