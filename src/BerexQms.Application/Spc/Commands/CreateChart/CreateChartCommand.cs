using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Spc.Commands.CreateChart;

public sealed record CreateChartCommand(
    string Code,
    string Name,
    string ChartType,
    Guid PartId,
    string CharacteristicName,
    int SubgroupSize,
    decimal? UpperSpecLimit,
    decimal? LowerSpecLimit) : ICommand<Guid>;
