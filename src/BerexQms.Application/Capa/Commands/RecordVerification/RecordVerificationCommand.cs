using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Capa.Commands.RecordVerification;

public sealed record RecordVerificationCommand(
    Guid CapaId,
    Guid VerificationId,
    bool IsEffective,
    string Result,
    string? Evidence) : ICommand;
