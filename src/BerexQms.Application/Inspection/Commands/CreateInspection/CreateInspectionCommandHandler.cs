using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Inspection.DTOs;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.Inspection.Entities;
using BerexQms.Domain.Inspection.Enums;
using BerexQms.Domain.Inspection.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Inspection.Commands.CreateInspection;

public sealed class CreateInspectionCommandHandler
    : ICommandHandler<CreateInspectionCommand, InspectionDto>
{
    private readonly IInspectionRepository _inspectionRepository;
    private readonly ITenantContext _tenantContext;

    public CreateInspectionCommandHandler(
        IInspectionRepository inspectionRepository,
        ITenantContext tenantContext)
    {
        _inspectionRepository = inspectionRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<InspectionDto>> Handle(
        CreateInspectionCommand request, CancellationToken cancellationToken)
    {
        if (await _inspectionRepository.InspectionNumberExistsAsync(
                request.InspectionNumber, cancellationToken))
            return InspectionErrors.InspectionNumberExists;

        if (!Enum.TryParse<InspectionType>(request.Type, true, out var inspectionType))
            return InspectionErrors.InvalidInspectionType;

        var record = InspectionRecord.Create(
            Guid.NewGuid(),
            _tenantContext.CurrentTenantId,
            request.InspectionNumber,
            inspectionType,
            request.PartId,
            request.PartRevisionId,
            request.LotNumber,
            request.LotSize,
            request.SampleSize,
            request.SupplierId,
            request.SamplingPlanId,
            request.InspectorId);

        await _inspectionRepository.AddAsync(record, cancellationToken);

        return new InspectionDto(
            record.Id,
            record.InspectionNumber,
            record.Type.ToString(),
            record.Status.ToString(),
            record.PartId,
            record.PartRevisionId,
            record.LotNumber,
            record.LotSize,
            record.SampleSize,
            record.SupplierId,
            record.InspectorId,
            null,
            record.CreatedAt);
    }
}
