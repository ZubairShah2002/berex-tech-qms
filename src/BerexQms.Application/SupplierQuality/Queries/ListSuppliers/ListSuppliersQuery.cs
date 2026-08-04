using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.SupplierQuality.DTOs;

namespace BerexQms.Application.SupplierQuality.Queries.ListSuppliers;

public sealed record ListSuppliersQuery(
    string? SearchTerm = null,
    string? Status = null,
    string? RiskLevel = null,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<SupplierDto>>;
