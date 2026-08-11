using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.AiEngine.Queries.GetGovernancePolicy;

/// <summary>
/// Get the tenant's AI governance policy. Administrator access only.
/// Returns defaults if no explicit policy exists.
/// </summary>
public sealed record GetGovernancePolicyQuery() : IQuery<AiGovernancePolicyDto>;
