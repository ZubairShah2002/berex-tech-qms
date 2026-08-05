using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Spc.Commands.DeactivateChart;

public sealed record DeactivateChartCommand(Guid Id) : ICommand;
