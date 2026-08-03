using BerexQms.Domain.AuditManagement.Enums;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.AuditManagement.Entities;

public sealed class AuditFinding : Entity<Guid>
{
    public Guid AuditRecordId { get; private set; }
    public FindingClassification Classification { get; private set; }
    public string ClauseReference { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? Evidence { get; private set; }
    public string? CorrectiveAction { get; private set; }
    public string? LinkedCapaId { get; private set; }
    public DateTime FoundAt { get; private set; }

    private AuditFinding() { }

    internal static AuditFinding Create(
        Guid id,
        TenantId tenantId,
        Guid auditRecordId,
        FindingClassification findingClassification,
        string clauseReference,
        string description,
        string? evidence,
        string? correctiveAction,
        string? linkedCapaId)
    {
        if (string.IsNullOrWhiteSpace(clauseReference))
            throw new DomainException("Clause reference is required.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Finding description is required.");

        return new AuditFinding
        {
            Id = id,
            TenantId = tenantId,
            AuditRecordId = auditRecordId,
            Classification = findingClassification,
            ClauseReference = clauseReference.Trim(),
            Description = description.Trim(),
            Evidence = evidence?.Trim(),
            CorrectiveAction = correctiveAction?.Trim(),
            LinkedCapaId = linkedCapaId?.Trim(),
            FoundAt = DateTime.UtcNow,
        };
    }
}
