using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.Spc.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Spc.Commands.UpdateChart;

internal sealed class UpdateChartCommandHandler : ICommandHandler<UpdateChartCommand>
{
    private readonly IControlChartRepository _repository;

    public UpdateChartCommandHandler(IControlChartRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(UpdateChartCommand request, CancellationToken cancellationToken)
    {
        var chart = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (chart is null)
            return Result.Failure(SpcErrors.ChartNotFound);

        chart.Update(request.Name, request.SubgroupSize, request.UpperSpecLimit, request.LowerSpecLimit);

        return Result.Success();
    }
}
