using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Application.NonConformance.DTOs;
using BerexQms.Domain.NonConformance.Entities;
using BerexQms.Domain.NonConformance.Enums;
using BerexQms.Domain.NonConformance.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.NonConformance.Commands.CreateNonConformance;

public sealed class CreateNonConformanceCommandHandler
    : ICommandHandler<CreateNonConformanceCommand, NonConformanceDto>
{
    private readonly INonConformanceRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CreateNonConformanceCommandHandler(
        INonConformanceRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<NonConformanceDto>> Handle(
        CreateNonConformanceCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.NcrNumberExistsAsync(request.NcrNumber, cancellationToken))
            return NonConformanceErrors.NcrNumberExists;

        if (!Enum.TryParse<NCSeverity>(request.Severity, true, out var severity))
            return NonConformanceErrors.InvalidSeverity;

        if (!Enum.TryParse<NCSource>(request.Source, true, out var source))
            return NonConformanceErrors.InvalidSource;

        if (!Enum.TryParse<DetectionPoint>(request.DetectionPoint, true, out var detectionPoint))
            return NonConformanceErrors.InvalidDetectionPoint;

        var record = NonConformanceRecord.Create(
            Guid.NewGuid(),
            _tenantContext.CurrentTenantId,
            request.NcrNumber,
            severity,
            source,
            detectionPoint,
            request.Description,
            request.PartId,
            request.PartRevisionId,
            request.LotNumber,
            request.SerialNumber,
            request.SupplierId,
            request.SupplierLotNumber,
            request.WorkOrderNumber,
            request.CustomerId,
            request.SourceInspectionId,
            request.QuantityAffected,
            request.QuantityDefective);

        await _repository.AddAsync(record, cancellationToken);

        return new NonConformanceDto(
            record.Id,
            record.NcrNumber,
            record.Status.ToString(),
            record.Severity.ToString(),
            record.Source.ToString(),
            record.DetectionPoint.ToString(),
            record.PartId,
            record.LotNumber,
            record.SupplierId,
            record.QuantityAffected,
            record.QuantityDefective,
            record.AssignedTo,
            record.CreatedAt);
    }
}
