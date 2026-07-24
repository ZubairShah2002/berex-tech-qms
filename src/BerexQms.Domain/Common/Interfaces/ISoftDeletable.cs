namespace BerexQms.Domain.Common.Interfaces;

/// <summary>
/// Marks a domain entity as supporting soft deletion rather than physical removal.
/// Quality records must never be physically deleted — they are marked as deleted
/// with full audit trail metadata for regulatory compliance and traceability.
/// EF Core global query filters use <see cref="IsDeleted"/> to exclude
/// soft-deleted records from standard queries.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Indicates whether the entity has been logically deleted.
    /// When <c>true</c>, the entity is excluded from standard queries
    /// but remains in the database for audit and compliance purposes.
    /// </summary>
    bool IsDeleted { get; }

    /// <summary>
    /// The UTC timestamp when the entity was soft-deleted.
    /// <c>null</c> when the entity has not been deleted.
    /// </summary>
    DateTime? DeletedAt { get; }

    /// <summary>
    /// The identifier of the user who performed the soft-delete operation.
    /// <c>null</c> when the entity has not been deleted.
    /// </summary>
    Guid? DeletedBy { get; }
}
