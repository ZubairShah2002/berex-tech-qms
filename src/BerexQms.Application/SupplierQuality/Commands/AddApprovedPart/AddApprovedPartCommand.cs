using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.SupplierQuality.DTOs;

namespace BerexQms.Application.SupplierQuality.Commands.AddApprovedPart;

public sealed record AddApprovedPartCommand(
    Guid SupplierId,
    Guid PartId,
    string? RevisionScope,
    DateTime ApprovalDate) : ICommand<ApprovedPartDto>;
