using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Training.Entities;

public sealed class TrainingCourse : AggregateRoot<Guid>, IAuditableEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal DurationHours { get; private set; }
    public string? AssessmentType { get; private set; }
    public string? PassCriteria { get; private set; }
    public Guid? QualificationId { get; private set; }
    public bool IsActive { get; private set; }

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private TrainingCourse() { }

    public static TrainingCourse Create(
        Guid id,
        TenantId tenantId,
        string code,
        string name,
        string? description,
        decimal durationHours,
        string? assessmentType,
        string? passCriteria,
        Guid? qualificationId)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Course code is required.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Course name is required.");
        if (durationHours <= 0)
            throw new DomainException("Course duration must be greater than zero.");

        return new TrainingCourse
        {
            Id = id,
            TenantId = tenantId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            DurationHours = durationHours,
            AssessmentType = assessmentType?.Trim(),
            PassCriteria = passCriteria?.Trim(),
            QualificationId = qualificationId,
            IsActive = true,
        };
    }

    public void Update(
        string name,
        string? description,
        decimal durationHours,
        string? assessmentType,
        string? passCriteria,
        Guid? qualificationId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Course name is required.");
        if (durationHours <= 0)
            throw new DomainException("Course duration must be greater than zero.");

        Name = name.Trim();
        Description = description?.Trim();
        DurationHours = durationHours;
        AssessmentType = assessmentType?.Trim();
        PassCriteria = passCriteria?.Trim();
        QualificationId = qualificationId;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
