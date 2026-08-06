using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.Spc.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Spc.Commands.AddDataPoint;

internal sealed class AddDataPointCommandHandler : ICommandHandler<AddDataPointCommand, Guid>
{
    private readonly IControlChartRepository _repository;

    public AddDataPointCommandHandler(IControlChartRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(AddDataPointCommand request, CancellationToken cancellationToken)
    {
        var chart = await _repository.GetByIdAsync(request.ChartId, cancellationToken);
        if (chart is null)
            return SpcErrors.ChartNotFound;

        if (!chart.IsActive)
            return SpcErrors.ChartInactive;

        var dataPoint = chart.AddDataPoint(
            request.Value,
            request.SubgroupValues,
            request.SampleSize,
            request.Timestamp,
            request.InspectionId);

        return dataPoint.Id;
    }
}
