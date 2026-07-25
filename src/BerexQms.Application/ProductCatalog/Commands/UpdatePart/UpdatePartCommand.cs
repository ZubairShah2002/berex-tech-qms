using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.ProductCatalog.DTOs;

namespace BerexQms.Application.ProductCatalog.Commands.UpdatePart;

public sealed record UpdatePartCommand(
    Guid PartId,
    string Name,
    string? Description,
    string? ProductFamily,
    string? Category,
    string SerializationMode,
    string? UnitOfMeasure) : ICommand<PartDto>;
