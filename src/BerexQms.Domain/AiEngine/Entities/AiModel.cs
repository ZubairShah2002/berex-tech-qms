using BerexQms.Domain.AiEngine.Enums;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.AiEngine.Entities;

/// <summary>
/// Model registry entry tracking an AI model's lifecycle
/// (Chapter 18.4): Data preparation -> Training -> Validation ->
/// Champion-Challenger (Shadow) -> Promotion (Active) -> Monitoring -> Retirement.
/// </summary>
public sealed class AiModel : AggregateRoot<Guid>, IAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Version { get; private set; } = string.Empty;
    public string Capability { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? TrainingMetrics { get; private set; }
    public string? ValidationMetrics { get; private set; }
    public string? HyperParameters { get; private set; }
    public string? DataSnapshotReference { get; private set; }
    public int? TrainingSampleCount { get; private set; }
    public DateTime? TrainedAt { get; private set; }
    public DateTime? PromotedAt { get; private set; }
    public DateTime? RetiredAt { get; private set; }

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private AiModel() { }

    public static AiModel Create(
        Guid id,
        TenantId tenantId,
        string name,
        string version,
        AiCapabilityType capability,
        string? description,
        string? trainingMetrics,
        string? hyperParameters,
        string? dataSnapshotReference,
        int? trainingSampleCount)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Model name is required.");
        if (string.IsNullOrWhiteSpace(version))
            throw new DomainException("Model version is required.");

        return new AiModel
        {
            Id = id,
            TenantId = tenantId,
            Name = name.Trim(),
            Version = version.Trim(),
            Capability = capability.ToString(),
            Status = ModelStatus.Training.ToString(),
            Description = description,
            TrainingMetrics = trainingMetrics,
            HyperParameters = hyperParameters,
            DataSnapshotReference = dataSnapshotReference,
            TrainingSampleCount = trainingSampleCount,
            TrainedAt = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Records the metrics produced by the validation phase. Typically called
    /// together with, or shortly before, <see cref="StartValidation"/>.
    /// </summary>
    public void RecordValidationMetrics(string validationMetrics)
    {
        if (string.IsNullOrWhiteSpace(validationMetrics))
            throw new DomainException("Validation metrics are required.");

        ValidationMetrics = validationMetrics;
    }

    public void StartValidation()
    {
        if (Status != ModelStatus.Training.ToString())
            throw new DomainException("Only a model in Training status can enter Validation.");

        Status = ModelStatus.Validating.ToString();
    }

    public void PromoteToShadow()
    {
        if (Status != ModelStatus.Validating.ToString())
            throw new DomainException("Only a model in Validating status can be promoted to Shadow.");

        Status = ModelStatus.Shadow.ToString();
    }

    public void Activate()
    {
        if (Status != ModelStatus.Shadow.ToString())
            throw new DomainException("Only a model in Shadow status can be activated as champion.");

        Status = ModelStatus.Active.ToString();
        PromotedAt = DateTime.UtcNow;
    }

    public void Deprecate()
    {
        if (Status != ModelStatus.Active.ToString())
            throw new DomainException("Only an Active model can be deprecated.");

        Status = ModelStatus.Deprecated.ToString();
    }

    public void Retire()
    {
        var currentStatus = Enum.Parse<ModelStatus>(Status);
        if (currentStatus is not (ModelStatus.Active or ModelStatus.Shadow or ModelStatus.Deprecated))
            throw new DomainException("Only an Active, Shadow, or Deprecated model can be retired.");

        Status = ModelStatus.Retired.ToString();
        RetiredAt = DateTime.UtcNow;
    }
}
