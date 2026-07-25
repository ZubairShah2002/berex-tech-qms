using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.ProductCatalog.DTOs;

namespace BerexQms.Application.ProductCatalog.Commands.CreatePartRevision;

public sealed record CreatePartRevisionCommand(
    Guid PartId,
    string RevisionCode,
    string? Description,
    string? ChangeReason) : ICommand<PartRevisionDto>;
