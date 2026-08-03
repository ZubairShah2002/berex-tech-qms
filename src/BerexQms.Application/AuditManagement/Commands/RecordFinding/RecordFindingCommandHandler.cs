using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AuditManagement.DTOs;
using BerexQms.Domain.AuditManagement.Enums;
using BerexQms.Domain.AuditManagement.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AuditManagement.Commands.RecordFinding;

internal sealed class RecordFindingCommandHandler : ICommandHandler<RecordFindingCommand, AuditFindingDto>
{
    private readonly IAuditRepository _repository;

    public RecordFindingCommandHandler(IAuditRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AuditFindingDto>> Handle(RecordFindingCommand request, CancellationToken cancellationToken)
    {
        var plan = await _repository.GetWithAuditsAsync(request.AuditPlanId, cancellationToken);
        if (plan is null)
            return AuditErrors.NotFound;

        if (!Enum.TryParse<FindingClassification>(request.Classification, true, out var classification))
            return Error.Validation("Audit.InvalidClassification", $"Invalid finding classification: {request.Classification}.");

        var finding = plan.RecordFinding(
            request.AuditRecordId, classification, request.ClauseReference,
            request.Description, request.Evidence, request.CorrectiveAction, request.LinkedCapaId);

        await _repository.UpdateAsync(plan, cancellationToken);

        return new AuditFindingDto(
            finding.Id,
            finding.AuditRecordId,
            finding.Classification.ToString(),
            finding.ClauseReference,
            finding.Description,
            finding.Evidence,
            finding.CorrectiveAction,
            finding.LinkedCapaId,
            finding.FoundAt);
    }
}
