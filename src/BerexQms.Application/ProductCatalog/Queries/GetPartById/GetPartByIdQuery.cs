using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.ProductCatalog.DTOs;

namespace BerexQms.Application.ProductCatalog.Queries.GetPartById;

public sealed record GetPartByIdQuery(Guid PartId) : IQuery<PartDetailDto>;
