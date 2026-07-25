using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.ProductCatalog.Commands.RemoveBomReference;

public sealed record RemoveBomReferenceCommand(Guid PartId, Guid BomReferenceId) : ICommand;
