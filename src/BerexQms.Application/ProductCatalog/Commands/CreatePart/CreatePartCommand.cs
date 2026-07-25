using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.ProductCatalog.DTOs;

namespace BerexQms.Application.ProductCatalog.Commands.CreatePart;

public sealed record CreatePartCommand(
    string PartNumber,
    string Name,
    string? Description,
    string? ProductFamily,
    string? Category,
    string SerializationMode,
    string? UnitOfMeasure) : ICommand<PartDto>;
