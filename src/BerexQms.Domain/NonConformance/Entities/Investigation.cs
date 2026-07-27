using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.NonConformance.Entities;

public sealed class Investigation : Entity<Guid>
{
    public Guid NonConformanceId { get; private set; }
    public string InvestigatorId { get; private set; } = string.Empty;
    public string? Methodology { get; private set; }
    public string? RootCause { get; private set; }
    public string? Findings { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private Investigation() { }

    internal static Investigation Create(
        Guid id,
        TenantId tenantId,
        Guid nonConformanceId,
        string investigatorId)
    {
        if (string.IsNullOrWhiteSpace(investigatorId))
            throw new DomainException("Investigator ID is required.");

        return new Investigation
        {
            Id = id,
            TenantId = tenantId,
            NonConformanceId = nonConformanceId,
            InvestigatorId = investigatorId,
            StartedAt = DateTime.UtcNow
        };
    }

    internal void SubmitFindings(string? methodology, string rootCause, string findings)
    {
        if (string.IsNullOrWhiteSpace(rootCause))
            throw new DomainException("Root cause is required.");

        if (string.IsNullOrWhiteSpace(findings))
            throw new DomainException("Investigation findings are required.");

        Methodology = methodology?.Trim();
        RootCause = rootCause.Trim();
        Findings = findings.Trim();
        CompletedAt = DateTime.UtcNow;
    }
}
