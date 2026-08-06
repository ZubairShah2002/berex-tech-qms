using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Enums;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Application.AiEngine.Specifications;

/// <summary>
/// Filters AI interactions by capability, status, and user action, with descending
/// request-time ordering and paging. Reused for both the paged listing and its matching
/// total-count query — paging is ignored automatically when used for a count.
/// </summary>
public sealed class AiInteractionFilterSpec : Specification<AiInteraction>
{
    public AiInteractionFilterSpec(
        string? capability, string? status, string? userAction, int page, int pageSize)
    {
        ApplyFilters(capability, status, userAction);
        ApplyOrderByDescending(i => i.RequestedAt);
        ApplyPaging(Math.Max(page - 1, 0) * pageSize, pageSize);
    }

    private void ApplyFilters(string? capability, string? status, string? userAction)
    {
        var hasCapability = !string.IsNullOrWhiteSpace(capability) &&
                             Enum.TryParse<AiCapabilityType>(capability, true, out _);
        var hasStatus = !string.IsNullOrWhiteSpace(status) &&
                         Enum.TryParse<AiInteractionStatus>(status, true, out _);
        var hasUserAction = !string.IsNullOrWhiteSpace(userAction) &&
                             Enum.TryParse<AiUserAction>(userAction, true, out _);

        if (!hasCapability && !hasStatus && !hasUserAction)
            return;

        var parsedCapability = hasCapability
            ? Enum.Parse<AiCapabilityType>(capability!, true).ToString()
            : null;
        var parsedStatus = hasStatus
            ? Enum.Parse<AiInteractionStatus>(status!, true).ToString()
            : null;
        var parsedUserAction = hasUserAction
            ? Enum.Parse<AiUserAction>(userAction!, true).ToString()
            : null;

        ApplyCriteria(i =>
            (!hasCapability || i.Capability == parsedCapability) &&
            (!hasStatus || i.Status == parsedStatus) &&
            (!hasUserAction || i.UserAction == parsedUserAction));
    }
}
