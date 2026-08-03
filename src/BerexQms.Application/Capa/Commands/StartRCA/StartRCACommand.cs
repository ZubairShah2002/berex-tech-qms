using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Capa.Commands.StartRCA;

public sealed record StartRCACommand(
    Guid CapaId,
    string Methodology,
    string AnalystId) : ICommand;
