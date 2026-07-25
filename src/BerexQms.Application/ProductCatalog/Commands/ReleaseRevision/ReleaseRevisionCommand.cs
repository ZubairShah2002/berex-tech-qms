using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.ProductCatalog.Commands.ReleaseRevision;

public sealed record ReleaseRevisionCommand(Guid PartId, Guid RevisionId) : ICommand;
