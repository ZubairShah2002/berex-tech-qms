using BerexQms.Domain.Capa.Enums;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Capa.Entities;

public sealed class CapaAction : Entity<Guid>
{
    public Guid CapaId { get; private set; }
    public ActionType ActionType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string OwnerId { get; private set; } = string.Empty;
    public DateTime DueDate { get; private set; }
    public string? EvidenceRequirement { get; private set; }
    public string? CompletionNotes { get; private set; }
    public string? EvidenceProvided { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? CompletedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private CapaAction() { }

    public static CapaAction Create(
        Guid id, TenantId tenantId, Guid capaId,
        ActionType actionType, string description, string ownerId,
        DateTime dueDate, string? evidenceRequirement)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Action description is required.");

        if (string.IsNullOrWhiteSpace(ownerId))
            throw new DomainException("Action owner is required.");

        if (dueDate <= DateTime.UtcNow.Date)
            throw new DomainException("Due date must be in the future.");

        return new CapaAction
        {
            Id = id,
            TenantId = tenantId,
            CapaId = capaId,
            ActionType = actionType,
            Description = description.Trim(),
            OwnerId = ownerId,
            DueDate = dueDate,
            EvidenceRequirement = evidenceRequirement?.Trim(),
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Complete(string completedBy, string? completionNotes, string? evidenceProvided)
    {
        if (CompletedAt is not null)
            throw new DomainException("Action has already been completed.");

        if (string.IsNullOrWhiteSpace(completedBy))
            throw new DomainException("Completed by is required.");

        CompletedBy = completedBy;
        CompletionNotes = completionNotes?.Trim();
        EvidenceProvided = evidenceProvided?.Trim();
        CompletedAt = DateTime.UtcNow;
    }

    public bool IsOverdue => CompletedAt is null && DueDate < DateTime.UtcNow;
}
