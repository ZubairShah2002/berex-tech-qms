using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Enums;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Application.AiEngine.Specifications;

/// <summary>
/// Filters AI models by capability and lifecycle status, with descending creation-date
/// ordering and paging. Reused for both the paged listing and its matching total-count
/// query — paging is ignored automatically when used for a count.
/// </summary>
public sealed class AiModelFilterSpec : Specification<AiModel>
{
    public AiModelFilterSpec(string? capability, string? status, int page, int pageSize)
    {
        ApplyFilters(capability, status);
        ApplyOrderByDescending(m => m.CreatedAt);
        ApplyPaging(Math.Max(page - 1, 0) * pageSize, pageSize);
    }

    private void ApplyFilters(string? capability, string? status)
    {
        var hasCapability = !string.IsNullOrWhiteSpace(capability) &&
                             Enum.TryParse<AiCapabilityType>(capability, true, out _);
        var hasStatus = !string.IsNullOrWhiteSpace(status) &&
                         Enum.TryParse<ModelStatus>(status, true, out _);

        if (!hasCapability && !hasStatus)
            return;

        var parsedCapability = hasCapability
            ? Enum.Parse<AiCapabilityType>(capability!, true).ToString()
            : null;
        var parsedStatus = hasStatus
            ? Enum.Parse<ModelStatus>(status!, true).ToString()
            : null;

        ApplyCriteria(m =>
            (!hasCapability || m.Capability == parsedCapability) &&
            (!hasStatus || m.Status == parsedStatus));
    }
}
