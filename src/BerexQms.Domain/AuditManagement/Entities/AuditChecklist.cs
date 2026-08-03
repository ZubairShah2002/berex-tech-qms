using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.AuditManagement.Entities;

public sealed class AuditChecklist : Entity<Guid>
{
    public Guid AuditRecordId { get; private set; }
    public string Standard { get; private set; } = string.Empty;
    public string ClauseReference { get; private set; } = string.Empty;
    public string Requirement { get; private set; } = string.Empty;
    public bool IsCompliant { get; private set; }
    public string? Evidence { get; private set; }
    public string? Notes { get; private set; }

    private AuditChecklist() { }

    internal static AuditChecklist Create(
        Guid id,
        TenantId tenantId,
        Guid auditRecordId,
        string standard,
        string clauseReference,
        string requirement,
        bool isCompliant,
        string? evidence,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(standard))
            throw new DomainException("Standard is required.");

        if (string.IsNullOrWhiteSpace(clauseReference))
            throw new DomainException("Clause reference is required.");

        if (string.IsNullOrWhiteSpace(requirement))
            throw new DomainException("Requirement is required.");

        return new AuditChecklist
        {
            Id = id,
            TenantId = tenantId,
            AuditRecordId = auditRecordId,
            Standard = standard.Trim(),
            ClauseReference = clauseReference.Trim(),
            Requirement = requirement.Trim(),
            IsCompliant = isCompliant,
            Evidence = evidence?.Trim(),
            Notes = notes?.Trim(),
        };
    }
}
