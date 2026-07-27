using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.NonConformance.DTOs;

namespace BerexQms.Application.NonConformance.Commands.AddContainmentAction;

public sealed record AddContainmentActionCommand(
    Guid NonConformanceId,
    string Description) : ICommand<ContainmentActionDto>;
