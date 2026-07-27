using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.NonConformance.Commands.SubmitInvestigation;

public sealed record SubmitInvestigationCommand(
    Guid NonConformanceId,
    string? Methodology,
    string RootCause,
    string Findings) : ICommand;
