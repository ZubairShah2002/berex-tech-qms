using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.NonConformance.Commands.AssignInvestigator;

public sealed record AssignInvestigatorCommand(
    Guid NonConformanceId,
    string InvestigatorId) : ICommand;
