using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Capa.Entities;

public sealed class EffectivenessVerification : Entity<Guid>
{
    public Guid CapaId { get; private set; }
    public DateTime ScheduledDate { get; private set; }
    public string VerificationCriteria { get; private set; } = string.Empty;
    public string? VerifierId { get; private set; }
    public string? Result { get; private set; }
    public string? Evidence { get; private set; }
    public bool? IsEffective { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private EffectivenessVerification() { }

    public static EffectivenessVerification Schedule(
        Guid id, TenantId tenantId, Guid capaId,
        DateTime scheduledDate, string verificationCriteria)
    {
        if (string.IsNullOrWhiteSpace(verificationCriteria))
            throw new DomainException("Verification criteria are required.");

        if (scheduledDate <= DateTime.UtcNow.Date)
            throw new DomainException("Verification date must be in the future.");

        return new EffectivenessVerification
        {
            Id = id,
            TenantId = tenantId,
            CapaId = capaId,
            ScheduledDate = scheduledDate,
            VerificationCriteria = verificationCriteria.Trim(),
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void RecordResult(string verifierId, bool isEffective, string result, string? evidence)
    {
        if (VerifiedAt is not null)
            throw new DomainException("Verification result has already been recorded.");

        if (string.IsNullOrWhiteSpace(verifierId))
            throw new DomainException("Verifier ID is required.");

        if (string.IsNullOrWhiteSpace(result))
            throw new DomainException("Verification result description is required.");

        VerifierId = verifierId;
        IsEffective = isEffective;
        Result = result.Trim();
        Evidence = evidence?.Trim();
        VerifiedAt = DateTime.UtcNow;
    }
}
