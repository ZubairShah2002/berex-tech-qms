using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Capa.Commands.SubmitRCA;

public sealed record SubmitRCACommand(
    Guid CapaId,
    string RootCause,
    string? AnalysisDetails,
    string? ContributingFactors) : ICommand;
