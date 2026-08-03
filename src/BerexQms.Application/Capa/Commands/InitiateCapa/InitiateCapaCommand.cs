using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Capa.Commands.InitiateCapa;

public sealed record InitiateCapaCommand(
    string CapaNumber,
    string Title,
    string Description,
    string Priority,
    string SourceType,
    Guid? SourceNonConformanceId,
    Guid? SourceAuditFindingId,
    string? SourceDescription,
    DateTime? TargetClosureDate) : ICommand<Guid>;
