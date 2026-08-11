using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.AiEngine.Queries.GetEffectivePolicy;

/// <summary>
/// Get the current user's effective AI policy — the resolved combination
/// of governance policy, role permissions, and user preferences.
/// </summary>
public sealed record GetEffectivePolicyQuery() : IQuery<AiEffectivePolicyDto>;
