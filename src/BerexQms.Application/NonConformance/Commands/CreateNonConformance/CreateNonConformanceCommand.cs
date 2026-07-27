using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.NonConformance.DTOs;

namespace BerexQms.Application.NonConformance.Commands.CreateNonConformance;

public sealed record CreateNonConformanceCommand(
    string NcrNumber,
    string Severity,
    string Source,
    string DetectionPoint,
    string Description,
    Guid PartId,
    Guid? PartRevisionId,
    string? LotNumber,
    string? SerialNumber,
    Guid? SupplierId,
    string? SupplierLotNumber,
    string? WorkOrderNumber,
    Guid? CustomerId,
    Guid? SourceInspectionId,
    int QuantityAffected,
    int QuantityDefective) : ICommand<NonConformanceDto>;
