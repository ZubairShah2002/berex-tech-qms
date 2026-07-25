using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.ProductCatalog.Commands.ObsoletePart;

public sealed record ObsoletePartCommand(Guid PartId) : ICommand;
