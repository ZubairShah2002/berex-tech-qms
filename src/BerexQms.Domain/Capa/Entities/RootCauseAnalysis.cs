using BerexQms.Domain.Capa.Enums;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Capa.Entities;

public sealed class RootCauseAnalysis : Entity<Guid>
{
    public Guid CapaId { get; private set; }
    public RCAMethodology Methodology { get; private set; }
    public string? AnalysisDetails { get; private set; }
    public string? RootCause { get; private set; }
    public string? ContributingFactors { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string AnalystId { get; private set; } = string.Empty;

    private RootCauseAnalysis() { }

    public static RootCauseAnalysis Create(
        Guid id, TenantId tenantId, Guid capaId,
        RCAMethodology methodology, string analystId)
    {
        if (string.IsNullOrWhiteSpace(analystId))
            throw new DomainException("Analyst ID is required.");

        return new RootCauseAnalysis
        {
            Id = id,
            TenantId = tenantId,
            CapaId = capaId,
            Methodology = methodology,
            AnalystId = analystId,
            StartedAt = DateTime.UtcNow,
        };
    }

    public void SubmitFindings(string rootCause, string? analysisDetails, string? contributingFactors)
    {
        if (string.IsNullOrWhiteSpace(rootCause))
            throw new DomainException("Root cause is required.");

        if (CompletedAt is not null)
            throw new DomainException("RCA has already been completed.");

        RootCause = rootCause.Trim();
        AnalysisDetails = analysisDetails?.Trim();
        ContributingFactors = contributingFactors?.Trim();
        CompletedAt = DateTime.UtcNow;
    }
}
