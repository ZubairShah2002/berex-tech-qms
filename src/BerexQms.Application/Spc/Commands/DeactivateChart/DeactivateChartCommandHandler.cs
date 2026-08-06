using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.Spc.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Spc.Commands.DeactivateChart;

internal sealed class DeactivateChartCommandHandler : ICommandHandler<DeactivateChartCommand>
{
    private readonly IControlChartRepository _repository;

    public DeactivateChartCommandHandler(IControlChartRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeactivateChartCommand request, CancellationToken cancellationToken)
    {
        var chart = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (chart is null)
            return Result.Failure(SpcErrors.ChartNotFound);

        chart.Deactivate();

        return Result.Success();
    }
}
