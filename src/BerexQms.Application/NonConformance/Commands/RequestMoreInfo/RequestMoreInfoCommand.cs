using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.NonConformance.Commands.RequestMoreInfo;

public sealed record RequestMoreInfoCommand(
    Guid NonConformanceId,
    string Reason) : ICommand;
