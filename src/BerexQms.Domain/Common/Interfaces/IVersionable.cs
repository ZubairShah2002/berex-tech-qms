namespace BerexQms.Domain.Common.Interfaces;

/// <summary>
/// Marks a domain entity as supporting optimistic concurrency control.
/// The <see cref="RowVersion"/> property is mapped as a concurrency token
/// in EF Core, ensuring that concurrent updates to the same entity are
/// detected and rejected with a <c>DbUpdateConcurrencyException</c>.
/// </summary>
public interface IVersionable
{
    /// <summary>
    /// An opaque version stamp managed by the database engine.
    /// In PostgreSQL this is backed by the <c>xmin</c> system column;
    /// in SQL Server by a <c>rowversion</c> / <c>timestamp</c> column.
    /// EF Core includes this value in the <c>WHERE</c> clause of <c>UPDATE</c>
    /// and <c>DELETE</c> statements to detect concurrent modifications.
    /// </summary>
    byte[] RowVersion { get; }
}
