using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.ProductCatalog.DTOs;

namespace BerexQms.Application.ProductCatalog.Commands.AddBomReference;

public sealed record AddBomReferenceCommand(
    Guid PartId,
    Guid ChildPartId,
    decimal Quantity,
    string? ReferenceDesignator) : ICommand<BomReferenceDto>;
