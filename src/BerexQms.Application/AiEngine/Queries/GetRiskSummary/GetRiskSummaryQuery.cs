using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Queries.GetRiskSummary;

public sealed record GetRiskSummaryQuery() : IQuery<RiskSummaryDto>;
