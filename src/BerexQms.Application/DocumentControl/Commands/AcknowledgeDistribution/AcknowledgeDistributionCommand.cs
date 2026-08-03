using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.DocumentControl.Commands.AcknowledgeDistribution;

public sealed record AcknowledgeDistributionCommand(
    Guid DocumentId,
    Guid DistributionId) : ICommand;
