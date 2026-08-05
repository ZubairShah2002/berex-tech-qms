using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.Spc.Entities;
using BerexQms.Domain.Spc.Enums;
using BerexQms.Domain.Spc.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Spc.Commands.CreateChart;

internal sealed class CreateChartCommandHandler : ICommandHandler<CreateChartCommand, Guid>
{
    private readonly IControlChartRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CreateChartCommandHandler(IControlChartRepository repository, ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateChartCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.CodeExistsAsync(request.Code, cancellationToken))
            return SpcErrors.ChartCodeExists;

        if (!Enum.TryParse<ChartType>(request.ChartType, true, out var chartType))
            return SpcErrors.InvalidChartType;

        var chart = ControlChart.Create(
            Guid.NewGuid(),
            _tenantContext.CurrentTenantId,
            request.Code,
            request.Name,
            chartType,
            request.PartId,
            request.CharacteristicName,
            request.SubgroupSize,
            request.UpperSpecLimit,
            request.LowerSpecLimit);

        await _repository.AddAsync(chart, cancellationToken);

        return chart.Id;
    }
}
