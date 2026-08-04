using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.SupplierQuality.DTOs;

namespace BerexQms.Application.SupplierQuality.Commands.AddApproval;

public sealed record AddApprovalCommand(
    Guid SupplierId,
    string ScopeDescription,
    DateTime ApprovedDate,
    DateTime? ExpiryDate,
    string? Conditions) : ICommand<SupplierApprovalDto>;
