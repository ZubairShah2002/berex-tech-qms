using BerexQms.Domain.AiEngine.Enums;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.AiEngine.Entities;

/// <summary>
/// Defines a reusable AI workflow template, such as "Generate Monthly Management
/// Review". Each workflow specifies the minimum permission level required, the
/// steps to execute, and the modules involved.
/// </summary>
public sealed class AiWorkflowDefinition : AggregateRoot<Guid>, IAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string MinimumPermissionLevel { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    /// <summary>
    /// JSON array describing the ordered steps of this workflow.
    /// Each step contains: stepName, module, actionType, description.
    /// </summary>
    public string StepsDefinition { get; private set; } = string.Empty;

    /// <summary>
    /// Comma-separated list of module names touched by this workflow.
    /// </summary>
    public string AffectedModules { get; private set; } = string.Empty;

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private AiWorkflowDefinition() { }

    public static AiWorkflowDefinition Create(
        Guid id,
        TenantId tenantId,
        string name,
        string? description,
        AiPermissionLevel minimumPermissionLevel,
        AiActionCategory category,
        string stepsDefinition,
        string affectedModules)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Workflow name is required.");
        if (string.IsNullOrWhiteSpace(stepsDefinition))
            throw new DomainException("Workflow steps definition is required.");
        if (string.IsNullOrWhiteSpace(affectedModules))
            throw new DomainException("Affected modules must be specified.");

        return new AiWorkflowDefinition
        {
            Id = id,
            TenantId = tenantId,
            Name = name.Trim(),
            Description = description?.Trim(),
            MinimumPermissionLevel = minimumPermissionLevel.ToString(),
            Category = category.ToString(),
            IsActive = true,
            StepsDefinition = stepsDefinition,
            AffectedModules = affectedModules,
        };
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("Workflow definition is already inactive.");

        IsActive = false;
    }

    public void Activate()
    {
        if (IsActive)
            throw new DomainException("Workflow definition is already active.");

        IsActive = true;
    }

    public void UpdateSteps(string stepsDefinition, string affectedModules)
    {
        if (string.IsNullOrWhiteSpace(stepsDefinition))
            throw new DomainException("Workflow steps definition is required.");

        StepsDefinition = stepsDefinition;
        AffectedModules = affectedModules;
    }
}
