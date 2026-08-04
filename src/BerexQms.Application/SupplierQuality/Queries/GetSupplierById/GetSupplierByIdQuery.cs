using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.SupplierQuality.DTOs;

namespace BerexQms.Application.SupplierQuality.Queries.GetSupplierById;

public sealed record GetSupplierByIdQuery(Guid SupplierId) : IQuery<SupplierDetailDto>;
