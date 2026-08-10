using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Queries.GetAiUsageSummary;

internal sealed class GetAiUsageSummaryQueryHandler
    : IQueryHandler<GetAiUsageSummaryQuery, AiUsageSummaryDto>
{
    private readonly IAiUsageService _usageService;

    public GetAiUsageSummaryQueryHandler(IAiUsageService usageService)
    {
        _usageService = usageService;
    }

    public async Task<Result<AiUsageSummaryDto>> Handle(
        GetAiUsageSummaryQuery request, CancellationToken cancellationToken)
    {
        var summary = await _usageService.GetUsageSummaryAsync(cancellationToken);
        return summary;
    }
}
