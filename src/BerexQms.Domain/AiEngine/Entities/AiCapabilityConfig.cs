using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Events;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.AiEngine.Entities;

/// <summary>
/// Per-tenant configuration for an AI capability: the kill switch that lets a
/// tenant disable an AI capability entirely, and the confidence thresholds
/// used to classify predictions as Low/Moderate/High/Very High. AI capabilities
/// are disabled by default until a tenant explicitly opts in.
/// </summary>
public sealed class AiCapabilityConfig : AggregateRoot<Guid>, IAuditableEntity
{
    public const decimal MinimumLowConfidenceThreshold = 0.20m;

    public string Capability { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }
    public decimal LowConfidenceThreshold { get; private set; } = 0.30m;
    public decimal ModerateConfidenceThreshold { get; private set; } = 0.60m;
    public decimal HighConfidenceThreshold { get; private set; } = 0.85m;

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private AiCapabilityConfig() { }

    public static AiCapabilityConfig Create(Guid id, TenantId tenantId, AiCapabilityType capability)
    {
        return new AiCapabilityConfig
        {
            Id = id,
            TenantId = tenantId,
            Capability = capability.ToString(),
            IsEnabled = false,
            LowConfidenceThreshold = 0.30m,
            ModerateConfidenceThreshold = 0.60m,
            HighConfidenceThreshold = 0.85m,
        };
    }

    public void Enable(Guid toggledByUserId)
    {
        if (IsEnabled)
            return;

        IsEnabled = true;
        AddDomainEvent(new AiCapabilityToggledEvent(Capability, true, toggledByUserId));
    }

    public void Disable(Guid toggledByUserId)
    {
        if (!IsEnabled)
            return;

        IsEnabled = false;
        AddDomainEvent(new AiCapabilityToggledEvent(Capability, false, toggledByUserId));
    }

    public void UpdateThresholds(decimal low, decimal moderate, decimal high)
    {
        if (low < MinimumLowConfidenceThreshold)
            throw new DomainException("Low confidence threshold cannot be lower than 0.20.");
        if (low >= moderate)
            throw new DomainException("Low confidence threshold must be less than the moderate confidence threshold.");
        if (moderate >= high)
            throw new DomainException("Moderate confidence threshold must be less than the high confidence threshold.");
        if (high > 1.0m)
            throw new DomainException("High confidence threshold cannot exceed 1.0.");

        LowConfidenceThreshold = low;
        ModerateConfidenceThreshold = moderate;
        HighConfidenceThreshold = high;
    }

    public ConfidenceLevel ClassifyConfidence(decimal score)
    {
        if (score <= LowConfidenceThreshold)
            return ConfidenceLevel.Low;
        if (score <= ModerateConfidenceThreshold)
            return ConfidenceLevel.Moderate;
        if (score <= HighConfidenceThreshold)
            return ConfidenceLevel.High;

        return ConfidenceLevel.VeryHigh;
    }
}
