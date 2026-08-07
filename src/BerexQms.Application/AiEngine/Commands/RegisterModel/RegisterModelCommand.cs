using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.AiEngine.Commands.RegisterModel;

public sealed record RegisterModelCommand(
    string Name,
    string Version,
    string Capability,
    string? Description,
    string? HyperParameters) : ICommand<Guid>;
