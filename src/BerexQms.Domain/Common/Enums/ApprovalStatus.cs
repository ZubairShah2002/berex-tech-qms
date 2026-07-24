namespace BerexQms.Domain.Common.Enums;

/// <summary>
/// Represents the approval lifecycle of a controlled document, procedure,
/// or other artifact that requires formal review and authorization before becoming effective.
/// </summary>
public enum ApprovalStatus
{
    /// <summary>
    /// The artifact is being authored or revised and has not yet been submitted for review.
    /// Only the author and designated collaborators can view and edit.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// The artifact has been submitted for peer or technical review.
    /// Reviewers can add comments and request changes.
    /// </summary>
    UnderReview = 1,

    /// <summary>
    /// The review cycle is complete and the artifact is awaiting formal approval
    /// from an authorized approver (typically requires an electronic signature).
    /// </summary>
    PendingApproval = 2,

    /// <summary>
    /// The artifact has been formally approved and is effective for use.
    /// Further changes require a new revision cycle.
    /// </summary>
    Approved = 3,

    /// <summary>
    /// The artifact was not approved during the review/approval cycle.
    /// Requires revision and resubmission to proceed.
    /// </summary>
    Rejected = 4
}
