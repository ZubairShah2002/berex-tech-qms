using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Spc.Commands.UpdateChart;

public sealed record UpdateChartCommand(
    Guid Id,
    string Name,
    int SubgroupSize,
    decimal? UpperSpecLimit,
    decimal? LowerSpecLimit) : ICommand;
