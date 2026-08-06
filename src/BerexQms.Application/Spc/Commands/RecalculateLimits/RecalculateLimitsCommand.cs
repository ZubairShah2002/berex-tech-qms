using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Spc.Commands.RecalculateLimits;

public sealed record RecalculateLimitsCommand(Guid ChartId) : ICommand;
