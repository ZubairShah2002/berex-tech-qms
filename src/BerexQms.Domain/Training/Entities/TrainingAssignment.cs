using BerexQms.Domain.Common.Events;
using BerexQms.Domain.Training.Enums;
using BerexQms.Domain.Training.ValueObjects;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Training.Entities;

public sealed class TrainingAssignment : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid EmployeeId { get; private set; }
    public Guid CourseId { get; private set; }
    public Guid AssignedBy { get; private set; }
    public DateTime AssignedDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public TrainingCompletion? Completion { get; private set; }

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private TrainingAssignment() { }

    public static TrainingAssignment Create(
        Guid id,
        TenantId tenantId,
        Guid employeeId,
        Guid courseId,
        Guid assignedBy,
        DateTime assignedDate,
        DateTime dueDate)
    {
        if (dueDate <= assignedDate)
            throw new DomainException("Due date must be after the assigned date.");

        return new TrainingAssignment
        {
            Id = id,
            TenantId = tenantId,
            EmployeeId = employeeId,
            CourseId = courseId,
            AssignedBy = assignedBy,
            AssignedDate = assignedDate,
            DueDate = dueDate,
            Status = AssignmentStatus.Assigned.ToString(),
        };
    }

    public void StartProgress()
    {
        if (Status != AssignmentStatus.Assigned.ToString())
            throw new DomainException($"Cannot start from status '{Status}'.");

        Status = AssignmentStatus.InProgress.ToString();
    }

    public void Complete(
        DateTime completionDate,
        decimal? score,
        AssessmentResult result,
        Guid? assessorId,
        string? evidenceRef,
        Guid? qualificationId,
        int? validityMonths)
    {
        if (Status != AssignmentStatus.Assigned.ToString() &&
            Status != AssignmentStatus.InProgress.ToString() &&
            Status != AssignmentStatus.Overdue.ToString())
            throw new DomainException($"Cannot complete from status '{Status}'.");

        Completion = new TrainingCompletion(
            completionDate,
            score,
            result.ToString(),
            assessorId,
            evidenceRef);

        Status = AssignmentStatus.Completed.ToString();

        if (result == AssessmentResult.Pass && qualificationId.HasValue && validityMonths.HasValue)
        {
            AddDomainEvent(new TrainingCompletedEvent(
                EmployeeId,
                CourseId,
                "Qualified",
                completionDate.AddMonths(validityMonths.Value)));
        }
    }

    public void MarkOverdue()
    {
        if (Status == AssignmentStatus.Assigned.ToString() ||
            Status == AssignmentStatus.InProgress.ToString())
        {
            Status = AssignmentStatus.Overdue.ToString();
        }
    }

    public void Cancel()
    {
        if (Status == AssignmentStatus.Completed.ToString())
            throw new DomainException("Cannot cancel a completed assignment.");

        Status = AssignmentStatus.Cancelled.ToString();
    }
}
