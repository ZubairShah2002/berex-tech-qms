using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.DocumentControl.DTOs;

namespace BerexQms.Application.DocumentControl.Commands.AddDistribution;

public sealed record AddDistributionCommand(
    Guid DocumentId,
    Guid VersionId,
    string RecipientId,
    DateTime ComplianceDeadline) : ICommand<DistributionDto>;
