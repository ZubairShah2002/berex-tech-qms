using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.Capa.Entities;
using BerexQms.Domain.Capa.Enums;
using BerexQms.Domain.Capa.Repositories;
using BerexQms.Domain.Capa.ValueObjects;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Capa.Commands.InitiateCapa;

public sealed class InitiateCapaCommandHandler : ICommandHandler<InitiateCapaCommand, Guid>
{
    private readonly ICAPARepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public InitiateCapaCommandHandler(
        ICAPARepository repository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(InitiateCapaCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.CapaNumberExistsAsync(request.CapaNumber, cancellationToken))
            return CAPAErrors.CapaNumberExists;

        if (!Enum.TryParse<CAPAPriority>(request.Priority, true, out var priority))
            return Error.Validation("CAPA.InvalidPriority", $"Invalid priority: {request.Priority}.");

        if (!Enum.TryParse<CAPASourceType>(request.SourceType, true, out var sourceType))
            return Error.Validation("CAPA.InvalidSourceType", $"Invalid source type: {request.SourceType}.");

        var source = new CAPASource(
            sourceType,
            request.SourceNonConformanceId,
            request.SourceAuditFindingId,
            request.SourceDescription);

        var record = CAPARecord.Initiate(
            Guid.NewGuid(),
            _tenantContext.CurrentTenantId,
            request.CapaNumber,
            request.Title,
            request.Description,
            priority,
            source,
            _currentUserService.Email,
            request.SourceNonConformanceId,
            request.TargetClosureDate);

        await _repository.AddAsync(record, cancellationToken);
        return record.Id;
    }
}
