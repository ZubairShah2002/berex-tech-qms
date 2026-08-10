using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Queries.GetAiUsageSummary;

public sealed record GetAiUsageSummaryQuery() : IQuery<AiUsageSummaryDto>;
