using BerexQms.SharedKernel.Results;

namespace BerexQms.Domain.Common.Interfaces;

/// <summary>
/// Marks a domain entity as having a state-machine-driven workflow.
/// Implementing entities define their own valid states via <typeparamref name="TStatus"/>
/// and enforce transition rules (guard conditions, role gates, and business invariants)
/// within the <see cref="TransitionTo"/> method.
/// </summary>
/// <typeparam name="TStatus">The enumeration type representing the workflow's valid states.</typeparam>
public interface IHasWorkflow<TStatus> where TStatus : Enum
{
    /// <summary>
    /// Gets the current state of the entity within its workflow lifecycle.
    /// </summary>
    TStatus CurrentStatus { get; }

    /// <summary>
    /// Attempts to transition the entity to the specified new status.
    /// Implementations must validate that the transition is permitted from
    /// the current status and that all guard conditions are satisfied.
    /// Returns a successful <see cref="Result"/> if the transition is valid,
    /// or a failure <see cref="Result"/> with a descriptive error if the transition is rejected.
    /// </summary>
    /// <param name="newStatus">The target status to transition to.</param>
    /// <returns>A <see cref="Result"/> indicating success or failure of the transition.</returns>
    Result TransitionTo(TStatus newStatus);
}
